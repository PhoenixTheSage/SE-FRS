# Linux build of libamd_fidelityfx_vk.so

AMD ships no Linux binary for FidelityFX SDK 1.1.4. `libamd_fidelityfx_vk.so` one level up
is built from the v1.1.4 source with these patches. It exposes the same upscale providers as
the signed Windows DLL: FSR 3.1.4 and FSR 2.3.3.

Built on WSL Ubuntu 26.04 (gcc 15.2, cmake 4.2, libvulkan-dev 1.4.341). It needs glibc 2.29+
and `libvulkan.so.1`.

## What the patches change

- `patch_cmake.py`: lets the SDK configure with Ninja on Linux. It works around the
  `CMAKE_GENERATOR_PLATFORM` handling and the Windows-only toolchain block, and fixes the
  shader-output list bug that Ninja rejects. It force-includes the CRT shim and leaves out
  the Win32 frame generation swapchain. It also doubles `FFX_SDK_DEFAULT_CONTEXT_SIZE`
  (the private contexts hold `wchar_t` names, which are 4 bytes on Linux instead of 2).
- `ffx_linux_compat.h`: maps the MSVC CRT names (`_countof`, `*_s`) onto standard C.
- `ffx_sc_wsl.sh`: runs AMD's Windows `FidelityFX_SC.exe` shader compiler through WSL
  interop. It translates `/mnt/<drive>` paths, so the source tree must live on a Windows drive.

Not included: the frame generation swapchain (`ffxCreateContext` for the
FRAMEGENERATIONSWAPCHAIN desc types). SE-FRS only uses upscaling.

## Rebuild

```bash
git clone --depth 1 --branch v1.1.4 https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK.git /mnt/c/<dir>/ffxsdk
# Put these scripts in /mnt/c/<dir>/tools (ffx_sc_wsl.sh expects ../ffxsdk).
python3 -I tools/patch_cmake.py ffxsdk "$PWD/tools/ffx_sc_wsl.sh"
cmake -S ffxsdk/ffx-api -B ~/ffxbuild -G Ninja -DCMAKE_BUILD_TYPE=Release -DFFX_API_BACKEND=VK_X64
cmake --build ~/ffxbuild -j16
strip --strip-unneeded -o libamd_fidelityfx_vk.so ~/ffxbuild/libamd_fidelityfx_vk.so
gcc -o smoke tools/smoke.c -ldl && ./smoke ./libamd_fidelityfx_vk.so
```
