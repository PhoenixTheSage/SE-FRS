using System;
using ClientPlugin.Frs;
using HarmonyLib;

namespace ClientPlugin.Patches;

/// <summary>
/// Loaded NVIDIA DLSS still Harmony-patches <c>DrawScene</c>. When NGX is
/// unavailable (GTX) it calls <c>RestoreOutputResolution</c> every frame and
/// fights FRS DRS: SetDRS 1129↔1920, reallocating GBuffer/Anomaly every draw.
/// </summary>
internal static class ForeignUpscalerDrsPatch
{
    private static bool _applied;

    internal static void TryApply(Harmony harmony)
    {
        if (_applied || harmony == null)
            return;
        _applied = true;

        var type = AccessTools.TypeByName("ClientPlugin.Dlss.DlssRuntime");
        if (type == null)
            return;

        PatchSkip(harmony, type, "RestoreOutputResolution");
        PatchSkip(harmony, type, "ApplyInternalResolution");
    }

    static void PatchSkip(Harmony harmony, Type type, string name)
    {
        var method = AccessTools.DeclaredMethod(type, name);
        if (method == null)
            return;
        harmony.Patch(method, prefix: new HarmonyMethod(typeof(ForeignUpscalerDrsPatch), nameof(SkipWhenFrsOwnsDrs)));
        DebugLog.Write("yield DLSS " + name + " while FRS owns DRS");
    }

    private static bool SkipWhenFrsOwnsDrs()
    {
        return !FrsRuntime.WantsFrs;
    }
}
