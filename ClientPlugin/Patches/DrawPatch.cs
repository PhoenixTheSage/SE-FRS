using System;
using ClientPlugin.Frs;
using HarmonyLib;
using VRageRender;

namespace ClientPlugin.Patches;

[HarmonyPatch(typeof(MyRender11), "Draw", typeof(bool))]
internal static class DrawPatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        try
        {
            GpuSupport.TryProbe();
            FrsRuntime.SnapshotOutputSize();
            FrsRuntime.EvaluatedThisFrame = false;
            FrsRuntime.BeginFrameResources();
            if (FrsRuntime.WantsFrs)
            {
                FrsRuntime.TryPrepareFrame();
                if (FrsRuntime.IsLive)
                {
                    Jitter.BeginFrame();
                    // Sprites record before DrawScene and must see the swapchain size.
                    FrsRuntime.RestoreViewportToOutput();
                }
            }
            AnomalyHook.SyncUpscaleClaim();
        }
        catch (Exception e)
        {
            DebugLog.Write("Draw prefix: " + e.GetType().Name + ": " + e.Message);
        }
    }
}
