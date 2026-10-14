using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ClientPlugin.Patches;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using VRage.Utils;

namespace ClientPlugin.Frs;

/// <summary>DX11 FSR 2 evaluate. Loaded only beside the main plugin on Windows.</summary>
public static class Dx11Entry
{
    static Harmony harmony;
    static Harmony deviceHarmony;
    static bool routed;
    static bool defaultsAdded;
    static bool failed;

    public static void OnUpdate()
    {
        if (failed)
            return;
        try
        {
            EnsurePaths();
            if (!routed)
            {
                routed = true;
                GpuSupport.TryProbe();
                GameAntiAliasing.CoerceUnsupported();
                harmony = new Harmony(Plugin.Name);
                harmony.PatchAll(typeof(Dx11Entry).Assembly);
                deviceHarmony = new Harmony(DeviceDisposePatch.HarmonyId);
                DeviceDisposePatch.Apply(deviceHarmony);
                AnomalyHook.Probe();
                AnomalyHook.ClaimUpscale();
                MyLog.Default.WriteLine("FRS plugin initialized. GPU: " + GpuSupport.StatusLine);
                DebugLog.Write("Harmony patched, plugin initialized GPU=" + GpuSupport.StatusLine);
            }
            GameAntiAliasing.AlignWithPeer();
            ForeignUpscalerDrsPatch.TryApply(harmony);
            FrsRuntime.NotifyPluginsReady();
        }
        catch (Exception e)
        {
            failed = true;
            var root = e.GetBaseException();
            MyLog.Default.Error("FRS DX11 update failed; device path disabled this session: " +
                                root.GetType().Name + ": " + root.Message);
        }
    }

    public static void OnDispose()
    {
        GameAntiAliasing.Reset();
        BillboardOutputPass.Reset();
        FrsRuntime.Shutdown();
        GpuSupport.Reset();
    }

    public static void PrepareConfig()
    {
        GpuSupport.TryProbe();
        GameAntiAliasing.AlignConfigWithGame();
        GameAntiAliasing.AlignWithPeer();
    }

    public static void AddSearchPath(string path) => FrsHost.AddSearchPath(path);

    public static string StatusText()
    {
        GpuSupport.TryProbe();
        return FrsStatus.CurrentText;
    }

    public static bool CanOffer() => GpuSupport.CanOfferFrs;

    public static bool DenyFrs() => GpuSupport.Probed && !GpuSupport.CanOfferFrs;

    public static void NotifyConfigChanged() => FrsRuntime.NotifyConfigChanged();

    public static void ApplyFromConfig() => GameAntiAliasing.ApplyFromConfig();

    public static AntiAliasingChoice DisplayedChoice(AntiAliasingChoice choice) => GameAntiAliasing.DisplayedChoice(choice);

    public static string ChoiceName(AntiAliasingChoice choice) => GameAntiAliasing.ChoiceName(choice);

    public static void ApplyFromPeer(string choice) => GameAntiAliasing.ApplyFromPeer(choice);

    public static void ApplyFromUi(AntiAliasingChoice choice) => GameAntiAliasing.ApplyFromUi(choice);

    public static void FillCombo(object combo) => GameAntiAliasing.FillAndBindPluginCombo(combo as MyGuiControlCombobox);

    static void EnsurePaths()
    {
        if (!defaultsAdded)
        {
            defaultsAdded = true;
            FrsHost.AddSearchPath(Path.GetDirectoryName(typeof(Plugin).Assembly.Location));
            try
            {
                var pulsar = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
                if (!string.IsNullOrEmpty(pulsar))
                {
                    FrsHost.AddSearchPath(Path.Combine(pulsar, "Legacy", "Local"));
                    FrsHost.AddSearchPath(Path.Combine(pulsar, "Interim", "Local"));
                    FrsHost.AddSearchPath(Path.Combine(pulsar, "Local"));
                }
            }
            catch
            {
                // ignored
            }
        }
        foreach (var path in Plugin.SearchPaths)
            FrsHost.AddSearchPath(path);
    }
}
