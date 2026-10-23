using System;
using ClientPlugin.Patches;
using HarmonyLib;
using VRage.Utils;

namespace ClientPlugin.Frs;

/// <summary>Game settings hooks belong to the shared plugin, independently of GPU APIs.</summary>
internal static class GraphicsAaHost
{
    static Harmony harmony;
    static bool failed;
    static string Id => Plugin.Name + ".GraphicsAA";

    public static void TryInstall()
    {
        if (harmony != null || failed || !(AnomalyRenderer.AllowDx11Device || AnomalyRenderer.AllowGuestWork))
            return;
        var candidate = new Harmony(Id);
        try
        {
            candidate.CreateClassProcessor(typeof(GraphicsOptionsAaPatch)).Patch();
            candidate.CreateClassProcessor(typeof(GraphicsOptionsClosePatch)).Patch();
            harmony = candidate;
        }
        catch (Exception e)
        {
            candidate.UnpatchAll(Id);
            failed = true;
            MyLog.Default?.WriteLine("AA graphics controls unavailable: " + e.GetBaseException().Message);
        }
    }

    public static void Reset()
    {
        harmony?.UnpatchAll(Id);
        harmony = null;
        failed = false;
        GameAntiAliasing.Reset();
    }
}