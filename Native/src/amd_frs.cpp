#include "amd_frs.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <d3d11.h>

#include <cstdio>
#include <cstring>
#include <vector>

#include "ffx_error.h"
#include "ffx_fsr2.h"
#include "dx11/ffx_fsr2_dx11.h"

namespace
{
    FfxFsr2Context g_context{};
    std::vector<char> g_scratch;
    ID3D11Device* g_device = nullptr;
    bool g_created = false;
    char g_lastError[1024] = "not initialized";

    void SetError(const char* text)
    {
        if (!text || !text[0])
            text = "unknown error";
        std::snprintf(g_lastError, sizeof(g_lastError), "%s", text);
    }

    void SetErrorCode(const char* what, FfxErrorCode code)
    {
        std::snprintf(g_lastError, sizeof(g_lastError), "%s (0x%08X)", what, (unsigned)code);
    }

    void OnMessage(FfxFsr2MsgType type, const wchar_t* message)
    {
        if (!message)
            return;
        char utf8[768];
        const int n = WideCharToMultiByte(CP_UTF8, 0, message, -1, utf8, sizeof(utf8), nullptr, nullptr);
        if (n <= 0)
            return;
        std::snprintf(g_lastError, sizeof(g_lastError), "%s %s",
                      type == FFX_FSR2_MESSAGE_TYPE_ERROR ? "FSR2 error:" : "FSR2:", utf8);
    }

    FfxResource Wrap(
        ID3D11Resource* resource,
        const wchar_t* name,
        FfxResourceStates state = FFX_RESOURCE_STATE_COMPUTE_READ)
    {
        FfxResource empty{};
        if (!resource || !g_created)
            return empty;
        return ffxGetResourceDX11(&g_context, resource, name, state);
    }

    uint32_t ToFsrFlags(uint32_t flags)
    {
        uint32_t out = 0;
        if (flags & FRS_FLAG_HDR)
            out |= FFX_FSR2_ENABLE_HIGH_DYNAMIC_RANGE;
        if (flags & FRS_FLAG_INVERTED_DEPTH)
            out |= FFX_FSR2_ENABLE_DEPTH_INVERTED;
        if (flags & FRS_FLAG_INFINITE_DEPTH)
            out |= FFX_FSR2_ENABLE_DEPTH_INFINITE;
        if (flags & FRS_FLAG_AUTO_EXPOSURE)
            out |= FFX_FSR2_ENABLE_AUTO_EXPOSURE;
        return out;
    }
}

extern "C" {

AMD_FRS_API const char* FrsLastError()
{
    return g_lastError;
}

AMD_FRS_API const char* FrsVersion()
{
    return "FidelityFX Super Resolution 2.2.1 DX11";
}

AMD_FRS_API int32_t FrsGetRenderResolution(
    int32_t quality,
    uint32_t displayWidth,
    uint32_t displayHeight,
    uint32_t* renderWidth,
    uint32_t* renderHeight)
{
    if (!renderWidth || !renderHeight || displayWidth == 0 || displayHeight == 0)
    {
        SetError("invalid render-resolution arguments");
        return FFX_ERROR_INVALID_ARGUMENT;
    }

    if (quality <= FRS_QUALITY_NATIVE_AA)
    {
        *renderWidth = displayWidth;
        *renderHeight = displayHeight;
        SetError("ok");
        return FFX_OK;
    }

    const auto mode = static_cast<FfxFsr2QualityMode>(quality);
    const FfxErrorCode code = ffxFsr2GetRenderResolutionFromQualityMode(
        renderWidth, renderHeight, displayWidth, displayHeight, mode);
    if (code != FFX_OK)
    {
        SetErrorCode("ffxFsr2GetRenderResolutionFromQualityMode", code);
        return code;
    }
    if (*renderWidth == 0)
        *renderWidth = 1;
    if (*renderHeight == 0)
        *renderHeight = 1;
    SetError("ok");
    return FFX_OK;
}

AMD_FRS_API int32_t FrsGetJitterOffset(
    int32_t index,
    int32_t renderWidth,
    int32_t displayWidth,
    float* outX,
    float* outY)
{
    if (!outX || !outY || renderWidth <= 0 || displayWidth <= 0)
    {
        SetError("invalid jitter arguments");
        return FFX_ERROR_INVALID_ARGUMENT;
    }
    const int32_t phase = ffxFsr2GetJitterPhaseCount(renderWidth, displayWidth);
    const FfxErrorCode code = ffxFsr2GetJitterOffset(outX, outY, index, phase);
    if (code != FFX_OK)
        SetErrorCode("ffxFsr2GetJitterOffset", code);
    return code;
}

AMD_FRS_API void FrsDestroy()
{
    if (g_created)
    {
        ffxFsr2ContextDestroy(&g_context);
        g_created = false;
    }
    g_scratch.clear();
    g_device = nullptr;
    std::memset(&g_context, 0, sizeof(g_context));
    SetError("shutdown");
}

AMD_FRS_API int32_t FrsCreate(const FrsCreateDesc* desc)
{
    if (!desc || !desc->device || desc->displayWidth == 0 || desc->displayHeight == 0)
    {
        SetError("invalid create arguments");
        return FFX_ERROR_INVALID_ARGUMENT;
    }

    FrsDestroy();

    auto* device = static_cast<ID3D11Device*>(desc->device);
    const size_t scratchSize = ffxFsr2GetScratchMemorySizeDX11();
    g_scratch.assign(scratchSize, 0);

    FfxFsr2ContextDescription ctx{};
    ctx.flags = ToFsrFlags(desc->flags);
    ctx.maxRenderSize.width = desc->displayWidth;
    ctx.maxRenderSize.height = desc->displayHeight;
    ctx.displaySize.width = desc->displayWidth;
    ctx.displaySize.height = desc->displayHeight;
    ctx.device = ffxGetDeviceDX11(device);
    ctx.fpMessage = &OnMessage;

    const FfxErrorCode iface = ffxFsr2GetInterfaceDX11(
        &ctx.callbacks, device, g_scratch.data(), g_scratch.size());
    if (iface != FFX_OK)
    {
        SetErrorCode("ffxFsr2GetInterfaceDX11", iface);
        g_scratch.clear();
        return iface;
    }

    const FfxErrorCode created = ffxFsr2ContextCreate(&g_context, &ctx);
    if (created != FFX_OK)
    {
        SetErrorCode("ffxFsr2ContextCreate", created);
        g_scratch.clear();
        std::memset(&g_context, 0, sizeof(g_context));
        return created;
    }

    g_device = device;
    g_created = true;
    SetError("ok");
    return FFX_OK;
}

AMD_FRS_API int32_t FrsDispatch(const FrsDispatchDesc* desc)
{
    if (!g_created)
    {
        SetError("FRS context is not created");
        return FFX_ERROR_NULL_DEVICE;
    }
    if (!desc || !desc->context || !desc->color || !desc->depth || !desc->output ||
        desc->renderWidth == 0 || desc->renderHeight == 0)
    {
        SetError("invalid dispatch arguments");
        return FFX_ERROR_INVALID_ARGUMENT;
    }

    FfxFsr2DispatchDescription dispatch{};
    dispatch.commandList = desc->context;
    dispatch.color = Wrap(static_cast<ID3D11Resource*>(desc->color), L"FRS_Color");
    dispatch.depth = Wrap(static_cast<ID3D11Resource*>(desc->depth), L"FRS_Depth");
    dispatch.motionVectors = Wrap(
        static_cast<ID3D11Resource*>(desc->motionVectors), L"FRS_MotionVectors");
    dispatch.reactive = Wrap(static_cast<ID3D11Resource*>(desc->reactive), L"FRS_Reactive");
    dispatch.output = Wrap(
        static_cast<ID3D11Resource*>(desc->output),
        L"FRS_Output",
        FFX_RESOURCE_STATE_UNORDERED_ACCESS);
    dispatch.jitterOffset.x = desc->jitterX;
    dispatch.jitterOffset.y = desc->jitterY;
    dispatch.motionVectorScale.x = desc->mvScaleX;
    dispatch.motionVectorScale.y = desc->mvScaleY;
    dispatch.renderSize.width = desc->renderWidth;
    dispatch.renderSize.height = desc->renderHeight;
    dispatch.enableSharpening = desc->enableSharpening != 0 && desc->sharpness > 0.f;
    dispatch.sharpness = desc->sharpness;
    dispatch.frameTimeDelta = desc->frameTimeDeltaMs > 0.f ? desc->frameTimeDeltaMs : 16.6f;
    dispatch.preExposure = desc->preExposure > 0.f ? desc->preExposure : 1.f;
    dispatch.reset = desc->reset != 0;
    dispatch.cameraNear = desc->cameraNear > 0.f ? desc->cameraNear : 0.05f;
    dispatch.cameraFar = desc->cameraFar > 0.f ? desc->cameraFar : 100000.f;
    dispatch.cameraFovAngleVertical = desc->cameraFovV > 0.f ? desc->cameraFovV : 1.047f;
    dispatch.viewSpaceToMetersFactor = 1.f;

    const FfxErrorCode code = ffxFsr2ContextDispatch(&g_context, &dispatch);
    if (code != FFX_OK)
        SetErrorCode("ffxFsr2ContextDispatch", code);
    else
        SetError("ok");
    return code;
}

} // extern "C"
