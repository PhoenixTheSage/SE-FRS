using System.Globalization;
using System.Text;

namespace ClientPlugin.Frs;

public static class FrsStatus
{
    public static string CurrentText
    {
        get
        {
            var sb = new StringBuilder();
            AppendGpu(sb);
            sb.AppendLine();
            AppendFrs(sb);
            sb.AppendLine();
            AppendConfig(sb);
            sb.AppendLine();
            AppendEvaluate(sb);
            sb.AppendLine();
            AnomalyHook.AppendStatus(sb);
            sb.AppendLine();
            AppendPaths(sb);
            return sb.ToString();
        }
    }

    static void AppendGpu(StringBuilder sb)
    {
        sb.Append("GPU  ").Append(GpuSupport.StatusLine);
        sb.Append(" · eligible ").AppendLine(Yes(GpuSupport.CanOfferFrs));
    }

    static void AppendFrs(StringBuilder sb)
    {
        sb.Append("FRS  ");
        if (FrsHost.IsReady)
            sb.Append("ready");
        else if (FrsHost.IsLoaded && FrsHost.IsSupported)
            sb.Append("loaded, not ready");
        else if (FrsHost.IsLoaded)
            sb.Append("loaded, unsupported");
        else
            sb.Append("not loaded");
        sb.Append(" · ").Append(FrsHost.FeatureIsHdr ? "HDR" : "SDR");
        sb.Append(" · ").AppendLine(FrsHost.CurrentPresetHint);

        var err = FrsHost.LastError;
        if (!string.IsNullOrEmpty(err) && !(FrsHost.IsReady && err == "not initialized"))
            sb.Append("     ").AppendLine(err);
    }

    static void AppendConfig(StringBuilder sb)
    {
        var cfg = Config.Current;
        sb.Append("AA   ").Append(cfg.AntiAliasing);
        sb.Append(" · ").Append(cfg.Mode);
        sb.Append(" · sharp ").AppendLine(cfg.Sharpness.ToString("0.00", CultureInfo.InvariantCulture));
        sb.Append("Res  ").Append(FrsRuntime.InternalWidth).Append('x').Append(FrsRuntime.InternalHeight);
        sb.Append(" → ").Append(FrsRuntime.OutputWidth).Append('x').Append(FrsRuntime.OutputHeight).AppendLine();
    }

    static void AppendEvaluate(StringBuilder sb)
    {
        sb.Append("Eval ");
        if (FrsRuntime.EvaluateCount <= 0)
            sb.AppendLine("none");
        else
        {
            sb.Append(FrsRuntime.LastEvaluateWasHdr ? "HDR" : "LDR");
            sb.Append(" ×").Append(FrsRuntime.EvaluateCount);
            if (!string.IsNullOrEmpty(FrsRuntime.LastEvaluatePath))
                sb.Append(" · ").Append(FrsRuntime.LastEvaluatePath);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(FrsRuntime.LastEvaluateColorDesc))
            sb.Append("     color ").AppendLine(FrsRuntime.LastEvaluateColorDesc);
        if (!string.IsNullOrEmpty(FrsRuntime.LastEvaluateDestDesc))
            sb.Append("     dest  ").AppendLine(FrsRuntime.LastEvaluateDestDesc);

        sb.Append("     jitter ")
            .Append(Jitter.FromFsr ? "fsr " : "halton ")
            .Append(Jitter.OffsetX.ToString("0.###", CultureInfo.InvariantCulture))
            .Append(',')
            .Append(Jitter.OffsetY.ToString("0.###", CultureInfo.InvariantCulture));
        sb.Append(" · swapchain HDR ").Append(Yes(FrsRuntime.IsHdrSwapchainLive));
        sb.Append(" · want HDR ").AppendLine(Yes(FrsRuntime.WantsHdrEvaluate));

        if (FrsRuntime.LastEvaluateFailed)
            sb.AppendLine("     last evaluate failed; falling back to a stretch blit");
    }

    static void AppendPaths(StringBuilder sb)
    {
        sb.AppendLine("Paths");
        FrsHost.AppendSearchPaths(sb, "     ");
#if DEBUG
        if (!string.IsNullOrEmpty(DebugLog.FilePath))
            sb.Append("     debug ").AppendLine(DebugLog.FilePath);
#endif
    }

    static string Yes(bool value) => value ? "yes" : "no";
}
