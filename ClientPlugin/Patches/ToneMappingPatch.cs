using System;
using ClientPlugin.Frs;
using HarmonyLib;
using VRage.Render11.Resources;
using VRage.Utils;
using VRageRender;

namespace ClientPlugin.Patches;

[HarmonyPatch(typeof(MyToneMapping), nameof(MyToneMapping.Run))]
internal static class ToneMappingPatch
{
    private static bool _exceptionLogged;

    // Skip Keen SDR (and HdrRender's scRGB prefix result) when HDR
    // evaluate is required. Same gate as FRS_FLAG_HDR create flags.
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(ref IBorrowedCustomTexture __result)
    {
        if (!FrsRuntime.IsLive || !FrsRuntime.WantsHdrEvaluate)
            return true;
        if (FrsRuntime.EvaluatedThisFrame)
        {
            __result = FrsRuntime.AcquireHdrOutput();
            return false;
        }
        try
        {
            if (!FrsRuntime.TryEvaluateHdrDisplay())
            {
                // Do not run Keen's SDR operator onto an scRGB dest — that
                // clips peaks and is what Show Status reported as LDR.
                DebugLog.Write("ToneMapping skip Keen SDR after HDR evaluate miss");
                __result = FrsRuntime.AcquireHdrOutput();
                return false;
            }
            __result = FrsRuntime.AcquireHdrOutput();
            return false;
        }
        catch (Exception e)
        {
            LogOnce(e);
            __result = FrsRuntime.AcquireHdrOutput();
            return false;
        }
    }

    // After Anomaly AfterTonemap (Priority.First). LDR evaluate when no
    // Display tenant. Do not consume HdrRender's scRGB output.
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    private static void Postfix(ref IBorrowedCustomTexture __result)
    {
        if (!FrsRuntime.IsLive || FrsRuntime.EvaluatedThisFrame || __result == null)
            return;
        if (FrsRuntime.WantsHdrEvaluate)
            return;
        if (__result.Size.X != FrsRuntime.InternalWidth || __result.Size.Y != FrsRuntime.InternalHeight)
            return;

        try
        {
            var dest = FrsRuntime.AcquireLdrOutput();
            if (dest == null)
                return;

            if (!FrsRuntime.TryEvaluate(dest, __result))
            {
                DebugLog.Write("ToneMapping LDR evaluate failed src=" + __result.Size +
                               " dest=" + dest.Size);
                return;
            }

            FrsRuntime.NoteLdrEvaluate(dest, __result);
            __result.Release();
            __result = dest;
            FrsRuntime.EvaluatedThisFrame = true;
            FrsRuntime.ApplyOutputSpace();
            try
            {
                AnomalyHook.NotifyUpscaleComplete(MyRender11.RC, dest);
            }
            finally
            {
                MyRender11.RC?.ClearState();
            }

            DebugLog.WriteFrame("ToneMapping LDR evaluate src=" + FrsRuntime.InternalWidth + "x" +
                                FrsRuntime.InternalHeight + " dest=" + dest.Size);
        }
        catch (Exception e)
        {
            LogOnce(e);
        }
    }

    private static void LogOnce(Exception e)
    {
        var message = e.GetType().Name + ": " + e.Message;
        if (!_exceptionLogged)
        {
            _exceptionLogged = true;
            MyLog.Default.Warning("FRS tone-mapping patch failed: " + message);
        }
        DebugLog.Write("ToneMapping threw " + e);
    }
}
