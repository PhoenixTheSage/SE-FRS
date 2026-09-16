using System;
using ClientPlugin.Frs;
using HarmonyLib;
using VRageRender;

namespace ClientPlugin.Patches;

[HarmonyPatch(typeof(MyRender11), nameof(MyRender11.DrawScene))]
internal static class DrawScenePatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        try
        {
            if (!FrsRuntime.WantsFrs || !FrsRuntime.TryPrepareFrame())
            {
                FrsRuntime.RestoreOutputResolution();
                return;
            }

            BillboardOutputPass.BeginDraw();
            DebugLog.WriteFrame("DrawScene internal " + FrsRuntime.InternalWidth + "x" + FrsRuntime.InternalHeight);
            FrsRuntime.ApplyInternalResolution();
            FrsRuntime.PinViewportToInternal();
        }
        catch (Exception e)
        {
            DebugLog.Write("DrawScene prefix: " + e.GetType().Name + ": " + e.Message);
            FrsRuntime.RestoreOutputResolution();
        }
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        var env = MyRender11.Environment;
        if (env != null)
            Jitter.Restore(env.Matrices);
        if (!FrsRuntime.IsLive)
            return;
        FrsRuntime.BindUnjitteredHudConstants();
        var output = FrsRuntime.OutputPixelSize();
        var rc = MyRender11.RC;
        if (rc != null && output.X > 0 && output.Y > 0)
            rc.SetViewport(0f, 0f, output.X, output.Y);
        BillboardOutputPass.TryDrawAfterSceneBlit();
    }
}
