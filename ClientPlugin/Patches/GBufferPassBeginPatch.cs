using ClientPlugin.Frs;
using HarmonyLib;
using VRageRender;

namespace ClientPlugin.Patches;

[HarmonyPatch(typeof(MyGBufferPass), "Begin")]
internal static class GBufferPassBeginPatch
{
    [HarmonyPrefix]
    private static void Prefix(MyGBufferPass __instance)
    {
        if (!FrsRuntime.IsLive)
            return;
        var env = MyRender11.Environment;
        if (env == null)
            return;
        __instance.ViewProjection = env.Matrices.ViewProjectionAt0;
        __instance.Projection = env.Matrices.Projection;
    }
}
