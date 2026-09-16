cmake_minimum_required(VERSION 3.20)
project(amd_frs LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "amd_frs is 64-bit only")
endif()

set(FFX_FSR2_API_DX11 ON CACHE BOOL "Build FSR 2 DX11 backend" FORCE)
set(FFX_FSR2_API_DX12 OFF CACHE BOOL "Build FSR 2 DX12 backend" FORCE)
set(FFX_FSR2_API_VK OFF CACHE BOOL "Build FSR 2 Vulkan backend" FORCE)
set(FSR2_BUILD_AS_DLL OFF CACHE BOOL "Build FSR2 API as static libs" FORCE)
set(FSR2_AUTO_COMPILE_SHADERS ON CACHE BOOL "Compile FSR2 shaders" FORCE)

add_subdirectory("${CMAKE_CURRENT_SOURCE_DIR}/third_party/FidelityFX-FSR2/src/ffx-fsr2-api"
                 "${CMAKE_CURRENT_BINARY_DIR}/ffx-fsr2-api")

add_library(amd_frs SHARED
    src/amd_frs.cpp
    src/amd_frs.h)

target_compile_definitions(amd_frs PRIVATE AMD_FRS_EXPORTS UNICODE _UNICODE)
target_include_directories(amd_frs PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/src"
    "${CMAKE_CURRENT_SOURCE_DIR}/third_party/FidelityFX-FSR2/src/ffx-fsr2-api")

target_link_libraries(amd_frs PRIVATE
    ffx_fsr2_api_x64
    ffx_fsr2_api_dx11_x64
    d3d11
    dxguid)

set_target_properties(amd_frs PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${CMAKE_SOURCE_DIR}/../Assets"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${CMAKE_SOURCE_DIR}/../Assets"
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO "${CMAKE_SOURCE_DIR}/../Assets"
    OUTPUT_NAME amd_frs)
