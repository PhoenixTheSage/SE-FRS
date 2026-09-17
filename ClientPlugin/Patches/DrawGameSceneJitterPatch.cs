using ClientPlugin.Frs;
using HarmonyLib;
using VRage.Render11.Render;
using VRageRender;

namespace ClientPlugin.Patches;

/// <summary>
/// FSR Halton belongs on the 3D raster only. <c>DrawGameScene</c> starts with
/// <c>PrepareGameScene</c> / <c>MyOffscreenRenderer</c> — Rich HUD paints those
/// targets with <c>frame_.Environment.view_projection_matrix</c>. Applying jitter
/// in a DrawGameScene prefix baked Halton into the overlay, then FSR amplified it
/// whenever internal != output.
/// </summary>
[HarmonyPatch(typeof(MyRender11), "DrawGameScene")]
internal static class DrawGameSceneJitterPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        var env = MyRender11.Environment;
        if (env != null)
            Jitter.Restore(env.Matrices);
    }
}

[HarmonyPatch(typeof(MyRenderScheduler), nameof(MyRenderScheduler.Init))]
internal static class RenderSchedulerJitterApplyPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix()
    {
        FrsRuntime.BindJitteredSceneConstants();
    }
}

[HarmonyPatch(typeof(MyRenderScheduler), nameof(MyRenderScheduler.Done))]
internal static class RenderSchedulerJitterRestorePatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix()
    {
        FrsRuntime.UnjitterFrameConstants();
    }
}
