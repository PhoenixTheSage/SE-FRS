using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using SharpDX.Direct3D11;
using VRage.Utils;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace ClientPlugin.Frs;

public static class FrsHost
{
    public static bool IsLoaded { get; private set; }
    public static bool IsSupported { get; private set; }
    public static bool IsReady { get; private set; }
    public static bool SupportKnown { get; private set; }
    public static string LastError { get; internal set; } = "not initialized";
    public static string CurrentPresetHint => FrsNative.NativeVersion() ?? "FSR 2.2.1 DX11";

    private static readonly List<string> SearchPaths = [];
    private static readonly Stopwatch FrameClock = Stopwatch.StartNew();
    private static long _lastDispatchTicks;
    private static bool _tornDown;
    private static IntPtr _device;
    private static Device _deviceOwner;
    private static bool _initBlocked;
    private static uint _lastOutW;
    private static uint _lastOutH;
    private static int _lastQuality = int.MinValue;
    private static bool _lastHdr;
    public static bool FeatureIsHdr => _lastHdr;

    public static void AddSearchPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        path = Path.GetFullPath(path);
        if (!SearchPaths.Contains(path))
        {
            SearchPaths.Add(path);
            DebugLog.Write("search path " + path);
            if (!IsLoaded)
                _initBlocked = false;
        }
    }

    public static string SearchPathSummary()
    {
        return SearchPaths.Count == 0 ? "(none)" : string.Join("; ", SearchPaths);
    }

    internal static void AppendSearchPaths(StringBuilder sb, string indent)
    {
        if (sb == null)
            return;
        indent ??= "";
        if (SearchPaths.Count == 0)
        {
            sb.Append(indent).AppendLine("(none)");
            return;
        }

        foreach (var path in SearchPaths)
            sb.Append(indent).AppendLine(path);
    }

    public static bool TryInit(Device device, string logPath)
    {
        if (IsLoaded)
            return IsSupported;
        if (_initBlocked)
            return false;
        if (device == null || device.IsDisposed)
        {
            LastError = "D3D11 device is not ready";
            DebugLog.Write("TryInit: " + LastError);
            return false;
        }

        GpuSupport.TryProbe();
        if (!GpuSupport.CanAttemptFrs)
        {
            LastError = GpuSupport.UnsupportedReason;
            SupportKnown = true;
            IsSupported = false;
            DebugLog.Write("TryInit blocked: " + LastError);
            return false;
        }

        var searchPath = FindDllDirectory();
        DebugLog.Write("FRS Init dllSearch=" + searchPath + " log=" + logPath);
        try
        {
            if (!FrsNative.TryLoad(searchPath, out var loadError))
            {
                LastError = loadError;
                MyLog.Default.Warning("FRS: " + LastError);
                DebugLog.Write("FRS load failed: " + LastError);
                _initBlocked = true;
                SupportKnown = true;
                IsSupported = false;
                return false;
            }
        }
        catch (Exception e)
        {
            LastError = "FRS load threw: " + e.GetType().Name + ": " + e.Message;
            MyLog.Default.Error("FRS: " + LastError);
            DebugLog.Write(LastError);
            _initBlocked = true;
            SupportKnown = true;
            IsSupported = false;
            return false;
        }

        IsLoaded = true;
        _device = device.NativePointer;
        _deviceOwner = device;
        _tornDown = false;
        IsSupported = true;
        SupportKnown = true;
        LastError = FrsNative.LastError;
        DebugLog.Write("FRS Init ok " + (FrsNative.NativeVersion() ?? "") + " from " + FrsNative.LoadedFrom);
        return true;
    }

    public static bool TrySetMode(
        FrsMode mode,
        uint outputWidth,
        uint outputHeight,
        out uint renderWidth,
        out uint renderHeight)
    {
        renderWidth = outputWidth;
        renderHeight = outputHeight;
        if (!IsSupported)
        {
            DebugLog.Write("TrySetMode skipped: not supported");
            return false;
        }

        var quality = ToQuality(mode);
        var hdr = FrsRuntime.WantsHdrEvaluate;
        if (IsReady && _lastQuality == quality &&
            _lastOutW == outputWidth && _lastOutH == outputHeight && _lastHdr == hdr)
        {
            renderWidth = (uint)FrsRuntime.InternalWidth;
            renderHeight = (uint)FrsRuntime.InternalHeight;
            return true;
        }

        DebugLog.Write("SetMode quality=" + quality + " hdr=" + (hdr ? 1 : 0) +
                       " out=" + outputWidth + "x" + outputHeight);
        if (FrsNative.GetRenderResolution(quality, outputWidth, outputHeight, out renderWidth, out renderHeight) != 0)
        {
            IsReady = false;
            LastError = FrsNative.LastError;
            DebugLog.Write("GetRenderResolution failed: " + LastError);
            return false;
        }

        var flags = FrsNative.FlagInvertedDepth | FrsNative.FlagInfiniteDepth | FrsNative.FlagAutoExposure;
        if (hdr)
            flags |= FrsNative.FlagHdr;
        var created = FrsNative.Create(_device, outputWidth, outputHeight, flags);
        if (created != 0)
        {
            IsReady = false;
            LastError = FrsNative.LastError;
            DebugLog.Write("Create failed: " + LastError);
            return false;
        }

        if (_lastHdr != hdr)
            FrsRuntime.NotifyHdrFeatureChanged();
        _lastQuality = quality;
        _lastOutW = outputWidth;
        _lastOutH = outputHeight;
        _lastHdr = hdr;
        IsReady = true;
        LastError = FrsNative.LastError;
        DebugLog.Write("SetMode ok render=" + renderWidth + "x" + renderHeight + " " + LastError);
        return true;
    }

    public static bool Evaluate(
        Device device,
        DeviceContext context,
        Resource color,
        Resource depth,
        IntPtr motionVectors,
        Resource output,
        float jitterX,
        float jitterY,
        int reset,
        float sharpness,
        uint renderWidth,
        uint renderHeight,
        float cameraNear,
        float cameraFar,
        float cameraFovV,
        IntPtr reactiveMask = default(IntPtr))
    {
        if (!IsReady)
            return false;
        if (device == null || context == null || color == null || depth == null || output == null)
        {
            LastError = "Evaluate missing device, color, depth, or output";
            return false;
        }

        var motion = FrsD3d.EnsureMotionOrZero(device, context, motionVectors, renderWidth, renderHeight);
        if (!FrsD3d.PrepareEvalOutput(device, output, out var evalOutput, out var copyBack, out var destDesc))
        {
            LastError = FrsNative.LastError;
            return false;
        }

        var now = FrameClock.ElapsedTicks;
        var dtMs = 16.6f;
        if (_lastDispatchTicks != 0)
            dtMs = (float)((now - _lastDispatchTicks) * 1000.0 / Stopwatch.Frequency);
        _lastDispatchTicks = now;
        if (dtMs < 1f)
            dtMs = 1f;
        if (dtMs > 100f)
            dtMs = 100f;

        var desc = new FrsNative.DispatchDesc
        {
            Context = context.NativePointer,
            Color = color.NativePointer,
            Depth = depth.NativePointer,
            MotionVectors = motion,
            Reactive = reactiveMask,
            Output = evalOutput,
            JitterX = jitterX,
            JitterY = jitterY,
            MvScaleX = 1f,
            MvScaleY = 1f,
            RenderWidth = renderWidth,
            RenderHeight = renderHeight,
            FrameTimeDeltaMs = dtMs,
            CameraNear = cameraNear,
            CameraFar = cameraFar,
            CameraFovV = cameraFovV,
            Sharpness = sharpness,
            Reset = reset,
            EnableSharpening = sharpness > 0.001f ? 1 : 0,
            PreExposure = 1f
        };

        FrsD3d.UnbindPipeline(context);
        var code = FrsNative.Dispatch(ref desc);
        LastError = FrsNative.LastError;
        if (code != 0)
        {
            DebugLog.Write("Evaluate failed reset=" + reset + " jitter=" + jitterX + "," + jitterY +
                           " render=" + renderWidth + "x" + renderHeight + " dest=" + destDesc + " " + LastError);
            return false;
        }

        if (copyBack)
            FrsD3d.CopyEvalOutput(context, output);
        DebugLog.WriteFrame("Evaluate ok reset=" + reset + " render=" + renderWidth + "x" + renderHeight);
        return true;
    }

    public static IntPtr GenerateCameraMotionVectors(
        Device device,
        DeviceContext context,
        Resource depth,
        uint width,
        uint height,
        float[] invViewProj, float[] unjitteredViewProj, float[] prevViewProj)
    {
        if (!IsLoaded)
            return IntPtr.Zero;
        var mv = FrsD3d.GenerateCameraMotionVectors(
            device, context, depth, width, height,
            invViewProj, unjitteredViewProj, prevViewProj);
        if (mv == IntPtr.Zero)
            DebugLog.Write(
                "GenerateCameraMotionVectors failed " + width + "x" + height + " " + FrsNative.LastError);
        return mv;
    }

    public static bool TryUpsampleDepth(Device device, DeviceContext context, Resource srcDepth, Resource destDepth)
    {
        if (!IsLoaded)
            return false;
        if (device == null || context == null || srcDepth == null || destDepth == null)
            return false;
        if (!FrsD3d.TryUpsampleDepth(device, context, srcDepth, destDepth))
        {
            DebugLog.Write("UpsampleDepth failed " + FrsNative.LastError);
            return false;
        }

        return true;
    }

    public static bool TryGetJitter(int index, int renderWidth, int displayWidth, out float x, out float y)
    {
        return FrsNative.TryGetJitter(index, renderWidth, displayWidth, out x, out y);
    }

    public static void AllowRetry()
    {
        _initBlocked = false;
        SupportKnown = IsLoaded;
    }

    public static void Shutdown()
    {
        DebugLog.Write(
            "FrsHost.Shutdown loaded=" + IsLoaded + " ready=" + IsReady + " tornDown=" + _tornDown);
        if (IsLoaded && !_tornDown)
        {
            try
            {
                FrsNative.Destroy();
                FrsD3d.Release();
            }
            catch (Exception e)
            {
                DebugLog.Write("FrsHost.Shutdown destroy: " + e.GetType().Name + ": " + e.Message);
            }
            _tornDown = true;
        }
        ResetSessionFlags();
    }

    public static void OnDeviceDisposing(Device device)
    {
        if (device == null || !ReferenceEquals(device, _deviceOwner))
            return;
        if (!IsLoaded || _tornDown)
            return;
        var pointer = device.NativePointer;
        if (pointer == IntPtr.Zero)
            return;
        if (_device != IntPtr.Zero && pointer != _device)
            return;

        DebugLog.Write("FrsHost.OnDeviceDisposing");
        try
        {
            FrsNative.Destroy();
            FrsD3d.Release();
        }
        catch (Exception e)
        {
            MyLog.Default.Error("FRS shutdown failed: " + e);
        }

        _tornDown = true;
        _device = IntPtr.Zero;
        _deviceOwner = null;
        ResetSessionFlags();
    }

    private static void ResetSessionFlags()
    {
        IsLoaded = false;
        IsSupported = false;
        IsReady = false;
        SupportKnown = false;
        _initBlocked = false;
        _lastOutW = 0;
        _lastOutH = 0;
        _lastQuality = int.MinValue;
        _lastHdr = false;
        _lastDispatchTicks = 0;
        LastError = "shutdown";
    }

    public static float FallbackScale(FrsMode mode)
    {
        switch (mode)
        {
            case FrsMode.NativeAA: return 1f;
            case FrsMode.Quality: return 2f / 3f;
            case FrsMode.Balanced: return 0.58f;
            case FrsMode.Performance: return 0.5f;
            case FrsMode.UltraPerformance: return 1f / 3f;
            default: return 2f / 3f;
        }
    }

    private static int ToQuality(FrsMode mode)
    {
        switch (mode)
        {
            case FrsMode.Performance: return FrsNative.QualityPerformance;
            case FrsMode.Balanced: return FrsNative.QualityBalanced;
            case FrsMode.Quality: return FrsNative.QualityQuality;
            case FrsMode.UltraPerformance: return FrsNative.QualityUltraPerformance;
            case FrsMode.NativeAA: return FrsNative.QualityNativeAa;
            default: return FrsNative.QualityQuality;
        }
    }

    private static string FindDllDirectory()
    {
        foreach (var path in SearchPaths)
            if (File.Exists(Path.Combine(path, "amd_frs.dll")))
                return path;
        return SearchPaths.Count > 0 ? SearchPaths[0] : Environment.CurrentDirectory;
    }
}
