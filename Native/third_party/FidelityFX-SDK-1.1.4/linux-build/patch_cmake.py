import pathlib, sys

root = pathlib.Path(sys.argv[1])
wrapper = sys.argv[2]

def patch(rel, old, new):
    p = root / rel
    s = p.read_text()
    if new and new in s:
        return
    if old not in s:
        sys.exit(f"pattern not found in {rel}: {old!r}")
    p.write_text(s.replace(old, new, 1))

# Ninja rejects CMAKE_GENERATOR_PLATFORM, so the SDK reads FFX_GEN_PLATFORM instead.
for p in list(root.glob("ffx-api/**/CMake*.txt")) + list(root.glob("sdk/**/CMake*.txt")) + [root / "common.cmake"]:
    s = p.read_text()
    if "CMAKE_GENERATOR_PLATFORM" in s:
        p.write_text(s.replace("CMAKE_GENERATOR_PLATFORM", "FFX_GEN_PLATFORM"))

api = "ffx-api/CMakeLists.txt"
patch(api, "project(FFX_API_${FFX_PLATFORM_NAME})",
      "project(FFX_API_${FFX_PLATFORM_NAME})\n"
      "if (UNIX)\n"
      "\tset(FFX_GEN_PLATFORM x64)\n"
      "\tset(CMAKE_POSITION_INDEPENDENT_CODE ON)\n"
      "else()\n"
      "\tset(FFX_GEN_PLATFORM ${CMAKE_GENERATOR_PLATFORM})\n"
      "endif()")
patch(api, "ffx_frameinterpolation_${FFX_GEN_PLATFORM} DXGI)",
      "ffx_frameinterpolation_${FFX_GEN_PLATFORM} $<$<PLATFORM_ID:Windows>:DXGI>)")
patch("sdk/CMakeLists.txt",
      "set(FFX_SC_EXECUTABLE ${CMAKE_CURRENT_SOURCE_DIR}/tools/binary_store/FidelityFX_SC.exe)",
      f"set(FFX_SC_EXECUTABLE {wrapper})")
# The toolchain forces a Windows/VS x64 target; skip that on a Linux host.
patch("sdk/toolchain.cmake",
      "if (FFX_API_BACKEND STREQUAL DX12_X64 OR\n\tFFX_API_BACKEND STREQUAL VK_X64)",
      "if (CMAKE_HOST_UNIX)\n\tset(CMAKE_RELWITHDEBINFO_POSTFIX drel)\n"
      "elseif (FFX_API_BACKEND STREQUAL DX12_X64 OR\n\tFFX_API_BACKEND STREQUAL VK_X64)")
# compile_shaders_with_depfile appends headers to the variable holding the output
# list's name, so that name lands in the dependency list. Ninja rejects it.
sc = root / "sdk/include/FidelityFX/gpu/CMakeCompileShaders.txt"
s = sc.read_text()
s = s.replace("list(APPEND PERMUTATION_OUTPUTS ", "list(APPEND _FFX_SC_OUTS ")
s = s.replace("set(${PERMUTATION_OUTPUTS} ${PERMUTATION_OUTPUTS} PARENT_SCOPE)",
              "set(${PERMUTATION_OUTPUTS} ${_FFX_SC_OUTS} PARENT_SCOPE)")
sc.write_text(s)
# Linux: force-include the CRT shim and leave out the Win32 frame generation swapchain.
compat = pathlib.Path(wrapper).parent / "ffx_linux_compat.h"
patch(api, "\tset(CMAKE_POSITION_INDEPENDENT_CODE ON)\n",
      "\tset(CMAKE_POSITION_INDEPENDENT_CODE ON)\n"
      f"\tset(CMAKE_CXX_FLAGS \"${{CMAKE_CXX_FLAGS}} -include {compat} -Wno-all\")\n"
      f"\tset(CMAKE_C_FLAGS \"${{CMAKE_C_FLAGS}} -include {compat}\")\n")
patch(api, "add_library(amd_fidelityfx_${FFX_PLATFORM_NAME} SHARED",
      "if (UNIX)\n"
      "\tset(private_ffx_api_backend_vk )\n"
      "endif()\n"
      "add_library(amd_fidelityfx_${FFX_PLATFORM_NAME} SHARED")
patch("sdk/src/backends/vk/CMakeLists.txt",
      "    \"${CMAKE_CURRENT_SOURCE_DIR}/FrameInterpolationSwapchain/*.h\"\n"
      "    \"${CMAKE_CURRENT_SOURCE_DIR}/FrameInterpolationSwapchain/*.cpp\"",
      "")

patch("ffx-api/include/ffx_api/ffx_api.h",
      "#define FFX_API_ENTRY __declspec(dllexport)",
      "#ifdef _WIN32\n#define FFX_API_ENTRY __declspec(dllexport)\n#else\n"
      "#define FFX_API_ENTRY __attribute__((visibility(\"default\")))\n#endif")

prov = "ffx-api/src/ffx_provider.cpp"
patch(prov, "#include <d3d12.h>\n", "#ifdef _WIN32\n#include <d3d12.h>\n#endif\n")
patch(prov, "#ifdef FFX_BACKEND_VK\n#include \"vk/ffx_provider_framegenerationswapchain_vk.h\"",
      "#if defined(FFX_BACKEND_VK) && defined(_WIN32)\n#include \"vk/ffx_provider_framegenerationswapchain_vk.h\"")
patch(prov, "#ifdef FFX_BACKEND_VK\n    &ffxProvider_FrameGenerationSwapChain_VK::Instance,",
      "#if defined(FFX_BACKEND_VK) && defined(_WIN32)\n    &ffxProvider_FrameGenerationSwapChain_VK::Instance,")
patch(prov, "MIDL_INTERFACE(\"b58d6601", "#ifdef _WIN32\nMIDL_INTERFACE(\"b58d6601")
patch(prov, "const ffxProvider* GetffxProvider(",
      "#else\nstatic void GetExternalProviders(void*, uint64_t) {}\n#endif\n\n"
      "const ffxProvider* GetffxProvider(")

# The backend table points at the swapchain's config hook; without the swapchain, report unsupported.
patch("sdk/src/backends/vk/ffx_vk.cpp",
      "    backendInterface->fpSwapChainConfigureFrameGeneration = ffxSetFrameGenerationConfigToSwapchainVK;",
      "#ifdef _WIN32\n"
      "    backendInterface->fpSwapChainConfigureFrameGeneration = ffxSetFrameGenerationConfigToSwapchainVK;\n"
      "#else\n"
      "    backendInterface->fpSwapChainConfigureFrameGeneration = [](FfxFrameGenerationConfig const*) -> FfxErrorCode { return FFX_ERROR_BACKEND_API_ERROR; };\n"
      "#endif")

# MSVC-only integer literal suffix.
for p in root.glob("ffx-api/src/*.cpp"):
    s = p.read_text()
    if "ui64" in s:
        p.write_text(s.replace("ui64", "ull"))

# Private contexts hold wchar_t names; wchar_t is 4 bytes on Linux, 2 on Windows.
types = root / "sdk/include/FidelityFX/host/ffx_types.h"
s = types.read_text()
import re
m = re.search(r"#define FFX_SDK_DEFAULT_CONTEXT_SIZE\s+\(([^\n]+)\)\n", s)
if not m:
    sys.exit("FFX_SDK_DEFAULT_CONTEXT_SIZE not found")
if "_WIN32" not in m.group(0):
    s = s.replace(m.group(0),
        "#ifdef _WIN32\n" + m.group(0) +
        "#else\n#define FFX_SDK_DEFAULT_CONTEXT_SIZE ((" + m.group(1) + ") * 2)\n#endif\n")
    types.write_text(s)
print("patched")
