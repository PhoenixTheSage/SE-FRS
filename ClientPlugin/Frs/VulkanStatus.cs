using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ClientPlugin.Frs;

/// <summary>
/// Show Status text for the Vulkan renderer. Section order and labels match
/// <see cref="FrsStatus"/> on the DX11 path.
/// </summary>
internal static class VulkanStatus
{
    const int VendorMicrosoft = 0x1414;
    const string UpscaleId = "se-frs";

    internal struct Snap
    {
        public string Gpu;
        public int Vendor;
        public int Ready;
        public int Supported;
        public bool IsHdr;
        public string State;
        public string Version;
        public uint Evals;
        public bool Failed;
        public string Path;
        public int InW, InH, OutW, OutH;
        public bool SwapHdr;
        public bool WantHdr;
        public float JitterX, JitterY;
        public bool History;
        public int MotionW, MotionH;
        public bool Feature;
    }

    static Type _catalog;
    static MethodInfo _catalogActive;
    static Type _owned;
    static MethodInfo _claim;
    static MethodInfo _release;
    static MethodInfo _notify;
    static PropertyInfo _hasDisplay;
    static PropertyInfo _upscaleId;
    static bool _scanned;

    public static string CurrentText
    {
        get
        {
            Sync(Config.Current != null && Config.Current.Enabled);
            var sb = new StringBuilder();
            var snap = Read();
            AppendGpu(sb, snap);
            sb.AppendLine();
            AppendFrs(sb, snap);
            sb.AppendLine();
            AppendConfig(sb, snap);
            sb.AppendLine();
            AppendEvaluate(sb, snap);
            sb.AppendLine();
            AppendAnomaly(sb, snap);
            sb.AppendLine();
            AppendPaths(sb);
            return sb.ToString();
        }
    }

    public static void Sync(bool enabled)
    {
        EnsureAnomaly();
        if (_claim == null)
            return;
        try
        {
            if (enabled)
                _claim.Invoke(null, new object[] { UpscaleId });
            else if (_release != null)
                _release.Invoke(null, new object[] { UpscaleId });
            if (enabled && _notify != null && IsEvaluating(Read().State))
                _notify.Invoke(null, null);
        }
        catch (Exception e)
        {
            DebugLog.Write("Vulkan status sync: " + e.GetType().Name + ": " + e.Message);
        }
    }

    internal static Snap Read()
    {
        var snap = new Snap
        {
            Gpu = "not detected yet",
            Version = "FSR 3.1.4",
            State = string.IsNullOrEmpty(FsrVulkan.ShortStatus) ? "off" : FsrVulkan.ShortStatus
        };
        string text;
        try
        {
            text = FsrVulkan.ReadDebug();
        }
        catch
        {
            return snap;
        }

        if (string.IsNullOrEmpty(text))
            return snap;
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            var eq = line.IndexOf('=');
            if (eq <= 0)
                continue;
            var key = line.Substring(0, eq);
            var value = line.Substring(eq + 1);
            switch (key)
            {
                case "gpu": snap.Gpu = value.Length == 0 ? "not detected yet" : value; break;
                case "vendor": snap.Vendor = ParseHex(value); break;
                case "ready": snap.Ready = ParseInt(value); break;
                case "supported": snap.Supported = ParseInt(value); break;
                case "ishdr": snap.IsHdr = ParseInt(value) != 0; break;
                case "state": snap.State = value; break;
                case "version": if (value.Length > 0) snap.Version = value; break;
                case "evals": snap.Evals = (uint)Math.Max(0, ParseInt(value)); break;
                case "failed": snap.Failed = ParseInt(value) != 0; break;
                case "path": snap.Path = value; break;
                case "in": SplitSize(value, out snap.InW, out snap.InH); break;
                case "out": SplitSize(value, out snap.OutW, out snap.OutH); break;
                case "hdr": snap.SwapHdr = ParseInt(value) != 0; break;
                case "want": snap.WantHdr = ParseInt(value) != 0; break;
                case "jitter": SplitJitter(value, out snap.JitterX, out snap.JitterY); break;
                case "history": snap.History = ParseInt(value) != 0; break;
                case "motion": SplitSize(value, out snap.MotionW, out snap.MotionH); break;
                case "feature": snap.Feature = ParseInt(value) != 0; break;
            }
        }

        return snap;
    }

    static void AppendGpu(StringBuilder sb, Snap snap)
    {
        sb.Append("GPU  ").Append(GpuLine(snap));
        sb.Append(" · eligible ").AppendLine(Eligible(snap) ? "yes" : "no");
    }

    static bool Eligible(Snap snap)
    {
        if (snap.Vendor == VendorMicrosoft || snap.Supported == 2)
            return false;
        return true;
    }

    static string GpuLine(Snap snap)
    {
        if (snap.Vendor == 0 || string.IsNullOrEmpty(snap.Gpu) || snap.Gpu == "not detected yet")
            return "not detected yet";
        return VendorName(snap.Vendor) + " " + snap.Gpu + " (0x" + snap.Vendor.ToString("X4") + ", Vulkan)";
    }

    static string VendorName(int vendor)
    {
        switch (vendor)
        {
            case 0x10DE: return "NVIDIA";
            case 0x1002: return "AMD";
            case 0x8086: return "Intel";
            case VendorMicrosoft: return "Microsoft";
            default: return "vendor 0x" + vendor.ToString("X4");
        }
    }

    static void AppendFrs(StringBuilder sb, Snap snap)
    {
        sb.Append("FRS  ");
        if (snap.Ready == 2)
            sb.Append("ready");
        else if (snap.Feature && snap.Supported != 2)
            sb.Append("loaded, not ready");
        else if (snap.Feature)
            sb.Append("loaded, unsupported");
        else
            sb.Append("not loaded");
        sb.Append(" · ").Append(snap.IsHdr ? "HDR" : "SDR");
        sb.Append(" · ").AppendLine(string.IsNullOrEmpty(snap.Version) ? "FSR 3.1.4" : snap.Version);
        if (IsError(snap.State))
            sb.Append("     ").AppendLine(snap.State);
    }

    static void AppendConfig(StringBuilder sb, Snap snap)
    {
        var cfg = Config.Current;
        sb.Append("AA   ").Append(cfg.AntiAliasing);
        sb.Append(" · ").Append(cfg.Mode);
        sb.Append(" · sharp ").AppendLine(cfg.Sharpness.ToString("0.00", CultureInfo.InvariantCulture));
        sb.Append("Res  ").Append(snap.InW).Append('x').Append(snap.InH);
        sb.Append(" → ").Append(snap.OutW).Append('x').Append(snap.OutH).AppendLine();
    }

    static void AppendEvaluate(StringBuilder sb, Snap snap)
    {
        sb.Append("Eval ");
        if (snap.Evals == 0 && string.IsNullOrEmpty(snap.Path))
            sb.AppendLine("none");
        else
        {
            sb.Append(snap.IsHdr ? "HDR" : "LDR");
            sb.Append(" ×").Append(snap.Evals);
            if (!string.IsNullOrEmpty(snap.Path))
                sb.Append(" · ").Append(snap.Path);
            sb.AppendLine();
            sb.Append("     color R16G16B16A16_Float ").Append(snap.InW).Append('x').Append(snap.InH).AppendLine();
            sb.Append("     dest  R16G16B16A16_Float ").Append(snap.OutW).Append('x').Append(snap.OutH).AppendLine();
        }

        sb.Append("     jitter ")
            .Append(snap.Ready == 2 ? "fsr " : "halton ")
            .Append(snap.JitterX.ToString("0.###", CultureInfo.InvariantCulture))
            .Append(',')
            .Append(snap.JitterY.ToString("0.###", CultureInfo.InvariantCulture));
        sb.Append(" · swapchain HDR ").Append(Yes(snap.SwapHdr));
        sb.Append(" · want HDR ").AppendLine(Yes(snap.WantHdr));
        if (snap.Failed)
            sb.AppendLine("     last evaluate failed; falling back to a stretch blit");
    }

    static void AppendAnomaly(StringBuilder sb, Snap snap)
    {
        if (snap.Evals == 0 && snap.MotionW <= 0 && string.IsNullOrEmpty(snap.Path))
            sb.AppendLine("Motion vectors: no evaluate attempted");
        else if (snap.MotionW <= 0)
            sb.AppendLine("Motion vectors: waiting");
        else
        {
            sb.Append("Evaluate #").Append(Math.Max(snap.Evals, 1));
            sb.Append(" frame=0 source=renderer fallback=none reset=");
            sb.Append(snap.History ? "none" : "history");
            sb.Append(" producerTexture=vulkan producerFrame=unavailable ");
            sb.AppendLine(snap.Failed ? "FRS bind failed" : snap.Evals > 0 ? "FRS bound" : "pending FRS binding");
        }

        AppendReactive(sb, snap);
        AppendUpscale(sb);
    }

    static void AppendReactive(StringBuilder sb, Snap snap)
    {
        EnsureAnomaly();
        if (_catalogActive == null && _owned == null)
        {
            sb.AppendLine("Reactive mask: Anomaly not loaded");
            return;
        }

        if (_catalogActive == null)
        {
            sb.AppendLine("Reactive mask: catalog not present");
            return;
        }

        if (!TryRead("reactiveMask", out var available, out var native, out var width, out var height) ||
            !available || native == IntPtr.Zero)
        {
            sb.AppendLine("Reactive mask: not published (no Reactive pack)");
            return;
        }

        var sizeOk = snap.InW <= 0 || (width == snap.InW && height == snap.InH);
        sb.Append("Reactive mask: ");
        sb.Append(sizeOk ? "Anomaly " : "skipped (size mismatch) ");
        sb.Append(width).Append('x').Append(height).AppendLine();
    }

    static void AppendUpscale(StringBuilder sb)
    {
        EnsureAnomaly();
        if (_notify == null && _owned == null)
        {
            sb.AppendLine("AfterUpscale: Anomaly not loaded");
            return;
        }

        if (_notify == null)
        {
            sb.AppendLine("AfterUpscale: notify API not present");
            return;
        }

        var id = "";
        var display = false;
        try
        {
            id = _upscaleId?.GetValue(null) as string ?? "";
            display = _hasDisplay?.GetValue(null) is true;
        }
        catch
        {
            id = "";
        }

        var evaluating = IsEvaluating(Read().State);
        sb.Append("AfterUpscale: ").Append(evaluating ? "notified" : "waiting");
        sb.Append(" · claim ").Append(string.IsNullOrEmpty(id) ? "none" : id);
        sb.Append(" · display ").AppendLine(display ? "yes" : "no");
    }

    static void AppendPaths(StringBuilder sb)
    {
        sb.AppendLine("Paths");
        var paths = Plugin.SearchPaths;
        if (paths == null || paths.Count == 0)
            sb.AppendLine("     (none)");
        else
        {
            foreach (var path in paths)
                sb.Append("     ").AppendLine(path);
        }
#if DEBUG
        if (!string.IsNullOrEmpty(DebugLog.FilePath))
            sb.Append("     debug ").AppendLine(DebugLog.FilePath);
#endif
    }

    static void EnsureAnomaly()
    {
        if (_scanned && (_catalogActive != null || _owned != null))
            return;
        _scanned = true;
        _owned = Find("ClientPlugin.Backend.OwnedPassRegistry") ?? Find("ClientPlugin.Shaders.OwnedPassRegistry");
        _catalog = Find("ClientPlugin.Buffers.BufferCatalog");
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        if (_owned != null)
        {
            _claim = _owned.GetMethod("ClaimUpscale", flags, null, new[] { typeof(string) }, null);
            _release = _owned.GetMethod("ReleaseUpscale", flags, null, new[] { typeof(string) }, null);
            _notify = _owned.GetMethod("NotifyUpscaleComplete", flags, null, Type.EmptyTypes, null);
            _hasDisplay = _owned.GetProperty("HasDisplayTenant", flags);
            _upscaleId = _owned.GetProperty("UpscaleConsumerId", flags);
        }

        if (_catalog != null)
            _catalogActive = _catalog.GetMethod("Active", flags, null, new[] { typeof(string) }, null);
    }

    static bool TryRead(string name, out bool available, out IntPtr native, out int width, out int height)
    {
        available = false;
        native = IntPtr.Zero;
        width = 0;
        height = 0;
        if (_catalogActive == null)
            return false;
        object buffer;
        try
        {
            buffer = _catalogActive.Invoke(null, new object[] { name });
        }
        catch
        {
            return false;
        }

        if (buffer == null)
            return false;
        var type = buffer.GetType();
        available = type.GetProperty("IsAvailable")?.GetValue(buffer) is true;
        if (type.GetProperty("NativeResource")?.GetValue(buffer) is IntPtr ptr)
            native = ptr;
        if (type.GetProperty("Width")?.GetValue(buffer) is int w)
            width = w;
        if (type.GetProperty("Height")?.GetValue(buffer) is int h)
            height = h;
        return true;
    }

    static Type Find(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type;
            try { type = assembly.GetType(name, false); }
            catch { continue; }
            if (type != null)
                return type;
        }

        return null;
    }

    static bool IsEvaluating(string state) =>
        !string.IsNullOrEmpty(state) && state.EndsWith(" evaluating", StringComparison.Ordinal);

    static bool IsError(string state)
    {
        if (string.IsNullOrEmpty(state) || state == "off")
            return false;
        if (state == "FSR waiting for motion" || IsEvaluating(state))
            return false;
        return true;
    }

    static int ParseInt(string text)
    {
        int value;
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
    }

    static int ParseHex(string text)
    {
        int value;
        return int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value) ? value : 0;
    }

    static void SplitSize(string text, out int w, out int h)
    {
        w = h = 0;
        var x = text.IndexOf('x');
        if (x <= 0)
            return;
        w = ParseInt(text.Substring(0, x));
        h = ParseInt(text.Substring(x + 1));
    }

    static void SplitJitter(string text, out float x, out float y)
    {
        x = y = 0f;
        var comma = text.IndexOf(',');
        if (comma <= 0)
            return;
        x = ParseInt(text.Substring(0, comma)) / 1000f;
        y = ParseInt(text.Substring(comma + 1)) / 1000f;
    }

    static string Yes(bool value) => value ? "yes" : "no";
}
