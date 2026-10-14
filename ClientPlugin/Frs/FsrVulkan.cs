using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ClientPlugin.Frs;

/// <summary>
/// Tells the Vulkan renderer where the FidelityFX library lives.
/// The renderer calls <c>ffxCreateContext</c> and <c>ffxDispatch</c> itself.
/// SDK 1.1.4 selects FSR 3.1.4. DX11 stays on FSR 2.2.1.
/// </summary>
internal static class FsrVulkan
{
    const string WindowsLibrary = "amd_fidelityfx_vk.dll";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int ConfigureFn(uint enabled, uint mode, float sharpness, IntPtr directory);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int StatusFn(IntPtr text, uint capacity);

    static ConfigureFn _configure;
    static StatusFn _status;
    static StatusFn _debug;
    static bool _missing;
    static string _lastStatus = "";

    internal static string ShortStatus => _lastStatus ?? "";

    internal static void Publish(bool enabled, FrsMode mode, float sharpness, IReadOnlyList<string> directories)
    {
        if (!TryResolve())
            return;
        var directory = enabled ? FindFeatureDirectory(directories) : null;
        var native = directory == null ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(directory);
        try
        {
            _configure(enabled && directory != null ? 1u : 0u, (uint)mode, sharpness, native);
        }
        finally
        {
            if (native != IntPtr.Zero)
                Marshal.FreeHGlobal(native);
        }

        var status = ReadStatus();
        if (status == _lastStatus)
            return;
        _lastStatus = status;
        try { VRage.Utils.MyLog.Default.WriteLine("FRS: " + status); }
        catch { }
        DebugLog.Write("FSR vulkan " + status);
    }

    static bool TryResolve()
    {
        if (_configure != null)
            return true;
        if (_missing)
            return false;
        var module = Module();
        if (module == IntPtr.Zero)
            return false;
        var configure = GetExport(module, "sevk_fsr_configure");
        var status = GetExport(module, "sevk_fsr_status");
        if (configure == IntPtr.Zero || status == IntPtr.Zero)
        {
            _missing = true;
            return false;
        }
        _configure = Marshal.GetDelegateForFunctionPointer<ConfigureFn>(configure);
        _status = Marshal.GetDelegateForFunctionPointer<StatusFn>(status);
        var debug = GetExport(module, "sevk_fsr_debug");
        if (debug != IntPtr.Zero)
            _debug = Marshal.GetDelegateForFunctionPointer<StatusFn>(debug);
        return true;
    }

    static IntPtr Module()
    {
        try
        {
            var module = GetModuleHandle("se_vulkan_native.dll");
            if (module != IntPtr.Zero)
                return module;
        }
        catch
        {
        }

#if NET
        if (!OperatingSystem.IsWindows() && Dlsym(IntPtr.Zero, "sevk_fsr_configure") != IntPtr.Zero)
            return new IntPtr(1);
#endif
        return IntPtr.Zero;
    }

    static IntPtr GetExport(IntPtr module, string name)
    {
        if (module == new IntPtr(1))
            return Dlsym(IntPtr.Zero, name);
        var exported = GetProcAddress(module, name);
        return exported != IntPtr.Zero ? exported : Dlsym(IntPtr.Zero, name);
    }

    internal static string FindFeatureDirectory(IReadOnlyList<string> directories)
    {
        if (directories != null)
        {
            foreach (var directory in directories)
            {
                if (ContainsFeature(directory))
                    return directory;
            }
        }

        var self = Path.GetDirectoryName(typeof(FsrVulkan).Assembly.Location);
        return ContainsFeature(self) ? self : null;
    }

    static bool ContainsFeature(string directory)
    {
        if (string.IsNullOrEmpty(directory))
            return false;
        try
        {
            if (File.Exists(Path.Combine(directory, WindowsLibrary)))
                return true;
            foreach (var _ in Directory.EnumerateFiles(directory, "libamd_fidelityfx_vk.so*"))
                return true;
        }
        catch
        {
            return false;
        }

        return false;
    }

    internal static string ReadDebug()
    {
        if (!TryResolve() || _debug == null)
            return null;
        return ReadText(_debug, 2048);
    }

    static string ReadStatus()
    {
        if (_status == null)
            return "";
        return ReadText(_status, 192);
    }

    static string ReadText(StatusFn fn, int bytes)
    {
        var buffer = Marshal.AllocHGlobal(bytes);
        try
        {
            for (var i = 0; i < bytes; i++)
                Marshal.WriteByte(buffer, i, 0);
            if (fn(buffer, (uint)bytes) != 0)
                return "";
            var copy = new byte[bytes];
            Marshal.Copy(buffer, copy, 0, bytes);
            var end = Array.IndexOf(copy, (byte)0);
            if (end < 0)
                end = copy.Length;
            return Encoding.UTF8.GetString(copy, 0, end);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr GetModuleHandle(string name);

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr module, string name);

    [DllImport("libdl.so.2")]
    static extern IntPtr dlsym(IntPtr handle, string symbol);

    static IntPtr Dlsym(IntPtr handle, string symbol)
    {
        try { return dlsym(handle, symbol); }
        catch { return IntPtr.Zero; }
    }
}
