#pragma once

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#ifdef AMD_FRS_EXPORTS
#define AMD_FRS_API __declspec(dllexport)
#else
#define AMD_FRS_API __declspec(dllimport)
#endif

enum FrsQuality
{
    FRS_QUALITY_NATIVE_AA = 0,
    FRS_QUALITY_QUALITY = 1,
    FRS_QUALITY_BALANCED = 2,
    FRS_QUALITY_PERFORMANCE = 3,
    FRS_QUALITY_ULTRA_PERFORMANCE = 4
};

enum FrsCreateFlags
{
    FRS_FLAG_HDR = 1 << 0,
    FRS_FLAG_INVERTED_DEPTH = 1 << 1,
    FRS_FLAG_INFINITE_DEPTH = 1 << 2,
    FRS_FLAG_AUTO_EXPOSURE = 1 << 3
};

typedef struct FrsCreateDesc
{
    void* device; // ID3D11Device*
    uint32_t displayWidth;
    uint32_t displayHeight;
    uint32_t flags;
} FrsCreateDesc;

typedef struct FrsDispatchDesc
{
    void* context; // ID3D11DeviceContext*
    void* color;   // ID3D11Resource*
    void* depth;
    void* motionVectors;
    void* reactive;
    void* output;
    float jitterX;
    float jitterY;
    float mvScaleX;
    float mvScaleY;
    uint32_t renderWidth;
    uint32_t renderHeight;
    float frameTimeDeltaMs;
    float cameraNear;
    float cameraFar;
    float cameraFovV;
    float sharpness;
    int32_t reset;
    int32_t enableSharpening;
    float preExposure;
} FrsDispatchDesc;

AMD_FRS_API int32_t FrsGetRenderResolution(
    int32_t quality,
    uint32_t displayWidth,
    uint32_t displayHeight,
    uint32_t* renderWidth,
    uint32_t* renderHeight);

AMD_FRS_API int32_t FrsCreate(const FrsCreateDesc* desc);
AMD_FRS_API int32_t FrsDispatch(const FrsDispatchDesc* desc);
AMD_FRS_API void FrsDestroy();
AMD_FRS_API int32_t FrsGetJitterOffset(
    int32_t index,
    int32_t renderWidth,
    int32_t displayWidth,
    float* outX,
    float* outY);
AMD_FRS_API const char* FrsLastError();
AMD_FRS_API const char* FrsVersion();

#ifdef __cplusplus
}
#endif
