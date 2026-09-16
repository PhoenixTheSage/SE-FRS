using System;
using System.IO;
using System.Runtime.InteropServices;
namespace ClientPlugin.Frs;

internal static class FrsNative
{
    internal const int QualityNativeAa = 0;
    internal const int QualityQuality = 1;
    internal const int QualityBalanced = 2;
    internal const int QualityPerformance = 3;
    internal const int QualityUltraPerformance = 4;

    internal const uint FlagHdr = 1 << 0;
    internal const uint FlagInvertedDepth = 1 << 1;
    internal const uint FlagInfiniteDepth = 1 << 2;
    internal const uint FlagAutoExposure = 1 << 3;

    private const string DllFileName = "amd_frs.dll";
    private static readonly object Gate = new();
    private static IntPtr _module;
    private static string _loadedFrom;
    private static string _lastError = "not initialized";

    internal static bool IsLoaded => _module != IntPtr.Zero;
    internal static string LoadedFrom => _loadedFrom;
    internal static string LastError => _lastError;

    internal static void SetError(string text)
    {
        _lastError = string.IsNullOrEmpty(text) ? "unknown error" : text;
    }

    internal static bool TryLoad(string directory, out string error)
    {
        lock (Gate)
        {
            if (_module != IntPtr.Zero)
            {
                error = null;
                return true;
            }

            var path = FindDll(directory);
            if (string.IsNullOrEmpty(path))
            {
                error = "amd_frs.dll was not found. Build Native/ or place the DLL next to the plugin.";
                SetError(error);
                return false;
            }

            var module = LoadLibrary(path);
            if (module == IntPtr.Zero)
            {
                error = "failed to load amd_frs.dll from " + path + " (Win32 " + Marshal.GetLastWin32Error() + ")";
                SetError(error);
                return false;
            }

            _module = module;
            _loadedFrom = path;
            SetError("loaded " + path);
            error = null;
            return true;
        }
    }

    internal static int GetRenderResolution(
        int quality, uint displayWidth, uint displayHeight, out uint renderWidth, out uint renderHeight)
    {
        renderWidth = displayWidth;
        renderHeight = displayHeight;
        try
        {
            var code = FrsGetRenderResolution(quality, displayWidth, displayHeight, out renderWidth, out renderHeight);
            SyncNativeError(code);
            return code;
        }
        catch (Exception e)
        {
            SetError("FrsGetRenderResolution threw: " + e.GetType().Name + ": " + e.Message);
            return -1;
        }
    }

    internal static int Create(IntPtr device, uint displayWidth, uint displayHeight, uint flags)
    {
        var desc = new CreateDesc
        {
            Device = device,
            DisplayWidth = displayWidth,
            DisplayHeight = displayHeight,
            Flags = flags
        };
        try
        {
            var code = FrsCreate(ref desc);
            SyncNativeError(code);
            return code;
        }
        catch (Exception e)
        {
            SetError("FrsCreate threw: " + e.GetType().Name + ": " + e.Message);
            return -1;
        }
    }

    internal static int Dispatch(ref DispatchDesc desc)
    {
        try
        {
            var code = FrsDispatch(ref desc);
            SyncNativeError(code);
            return code;
        }
        catch (Exception e)
        {
            SetError("FrsDispatch threw: " + e.GetType().Name + ": " + e.Message);
            return -1;
        }
    }

    internal static bool TryGetJitter(int index, int renderWidth, int displayWidth, out float x, out float y)
    {
        x = 0f;
        y = 0f;
        if (_module == IntPtr.Zero || renderWidth <= 0 || displayWidth <= 0)
            return false;
        try
        {
            return FrsGetJitterOffset(index, renderWidth, displayWidth, out x, out y) == 0;
        }
        catch
        {
            return false;
        }
    }

    internal static void Destroy()
    {
        if (_module == IntPtr.Zero)
            return;
        try
        {
            FrsDestroy();
        }
        catch (Exception e)
        {
            SetError("FrsDestroy threw: " + e.GetType().Name + ": " + e.Message);
        }
    }

    internal static string NativeVersion()
    {
        if (_module == IntPtr.Zero)
            return null;
        try
        {
            var ptr = FrsVersion();
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }
        catch
        {
            return null;
        }
    }

    private static void SyncNativeError(int code)
    {
        try
        {
            var ptr = FrsLastErrorPtr();
            if (ptr != IntPtr.Zero)
            {
                var text = Marshal.PtrToStringAnsi(ptr);
                if (!string.IsNullOrEmpty(text))
                    _lastError = code == 0 ? text : text;
            }
        }
        catch
        {
            if (code != 0)
                SetError("FRS native error 0x" + ((uint)code).ToString("X8"));
        }
    }

    private static string FindDll(string directory)
    {
        if (!string.IsNullOrEmpty(directory))
        {
            var candidate = Path.Combine(directory, DllFileName);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }
        return null;
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsGetRenderResolution")]
    private static extern int FrsGetRenderResolution(
        int quality, uint displayWidth, uint displayHeight, out uint renderWidth, out uint renderHeight);

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsCreate")]
    private static extern int FrsCreate(ref CreateDesc desc);

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsDispatch")]
    private static extern int FrsDispatch(ref DispatchDesc desc);

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsDestroy")]
    private static extern void FrsDestroy();

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsGetJitterOffset")]
    private static extern int FrsGetJitterOffset(
        int index, int renderWidth, int displayWidth, out float outX, out float outY);

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsLastError")]
    private static extern IntPtr FrsLastErrorPtr();

    [DllImport("amd_frs", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FrsVersion")]
    private static extern IntPtr FrsVersion();

    [StructLayout(LayoutKind.Sequential)]
    internal struct CreateDesc
    {
        public IntPtr Device;
        public uint DisplayWidth;
        public uint DisplayHeight;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DispatchDesc
    {
        public IntPtr Context;
        public IntPtr Color;
        public IntPtr Depth;
        public IntPtr MotionVectors;
        public IntPtr Reactive;
        public IntPtr Output;
        public float JitterX;
        public float JitterY;
        public float MvScaleX;
        public float MvScaleY;
        public uint RenderWidth;
        public uint RenderHeight;
        public float FrameTimeDeltaMs;
        public float CameraNear;
        public float CameraFar;
        public float CameraFovV;
        public float Sharpness;
        public int Reset;
        public int EnableSharpening;
        public float PreExposure;
    }
}
