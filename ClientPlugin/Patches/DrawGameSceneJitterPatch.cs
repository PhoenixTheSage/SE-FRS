using ClientPlugin.Frs;
using HarmonyLib;
using VRageRender;

namespace ClientPlugin.Patches;

[HarmonyPatch(typeof(MyRender11), "DrawGameScene")]
internal static class DrawGameSceneJitterPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix()
    {
        if (!FrsRuntime.IsLive)
            return;
        var env = MyRender11.Environment;
        if (env != null)
            Jitter.Apply(env.Matrices);
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        var env = MyRender11.Environment;
        if (env != null)
            Jitter.Restore(env.Matrices);
    }
}
