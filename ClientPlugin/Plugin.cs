using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ClientPlugin.Frs;
using ClientPlugin.Patches;
using ClientPlugin.RichHud;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using VRage.Plugins;
using VRage.Utils;

#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public sealed class Plugin : IPlugin
{
    public const string Name = "SpaceEngineersFRS";
    public static Plugin Instance { get; private set; }

    private SettingsGenerator settingsGenerator;
    private Harmony harmony;
    private Harmony deviceHarmony;
    private bool disposed;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        disposed = false;
        Instance = this;
        settingsGenerator = new SettingsGenerator();
        DebugLog.Open();
        AddDefaultSearchPaths();
        DebugLog.Write("Init search=" + FrsHost.SearchPathSummary());

        GpuSupport.TryProbe();
        GameAntiAliasing.CoerceUnsupported();
        AnomalyHook.Probe();
        AnomalyTerminalHook.TryInstall();

        harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        deviceHarmony = new Harmony(DeviceDisposePatch.HarmonyId);
        DeviceDisposePatch.Apply(deviceHarmony);
        AnomalyHook.Probe();
        AnomalyHook.ClaimUpscale();
        AnomalyTerminalHook.TryInstall();
        MyLog.Default.WriteLine("FRS plugin initialized. GPU: " + GpuSupport.StatusLine);
        DebugLog.Write("Harmony patched, plugin initialized GPU=" + GpuSupport.StatusLine);
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        DebugLog.Write("Dispose");
        GameAntiAliasing.Reset();
        BillboardOutputPass.Reset();
        harmony = null;
        // Leave Harmony patches in place. Pulsar only disposes plugins at
        // process exit; UnpatchAll rewrites shared trampolines while other
        // plugins may still be running. DeviceDisposePatch stays applied so
        // FRS can shut down when the D3D device is released.
        ConfigStorage.FlushPending(true);
        FrsRuntime.Shutdown();
        AnomalyTerminalHook.Reset();
        GpuSupport.Reset();
        settingsGenerator = null;
        if (ReferenceEquals(Instance, this))
            Instance = null;
        DebugLog.Close();
    }

    public void Update()
    {
        if (disposed)
            return;
        // Pulsar finishes every plugin Init before the first Update. FRS D3D11
        // init must not overlap Anomaly (or other plugins) Harmony.PatchAll.
        GameAntiAliasing.AlignWithPeer();
        AnomalyTerminalHook.TryInstall();
        ConfigStorage.FlushPending();
        ForeignUpscalerDrsPatch.TryApply(harmony);
        FrsRuntime.NotifyPluginsReady();
    }

    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        var generator = settingsGenerator;
        if (disposed || generator == null)
            return;

        GpuSupport.TryProbe();
        GameAntiAliasing.AlignConfigWithGame();
        GameAntiAliasing.AlignWithPeer();
        generator.SetLayout<Simple>();
        generator.Dialog.RecreateControls(true);
        MyGuiSandbox.AddScreen(generator.Dialog);
    }

    // ReSharper disable once UnusedMember.Global
    public void LoadAssets(IReadOnlyDictionary<string, string> assets)
    {
        if (disposed || assets == null)
            return;

        foreach (var pair in assets)
            AddAssetSearchPath(pair.Value, pair.Key);
        AnomalyTerminalHook.TryInstall();
    }

    // Older Pulsar still calls this when an asset is named AssetFolder.
    // ReSharper disable once UnusedMember.Global
    public void LoadAssets(string folder)
    {
        AddAssetSearchPath(folder, null);
    }

    private void AddAssetSearchPath(string path, string name)
    {
        if (disposed || string.IsNullOrEmpty(path))
            return;

        if (File.Exists(path))
            path = Path.GetDirectoryName(path);

        FrsHost.AddSearchPath(path);
        var label = string.IsNullOrEmpty(name) ? path : name + "=" + path;
        MyLog.Default.WriteLine("FRS asset: " + label);
        DebugLog.Write("LoadAssets " + label);
    }

    static void AddDefaultSearchPaths()
    {
        FrsHost.AddSearchPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        try
        {
            var pulsar = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
            if (string.IsNullOrEmpty(pulsar))
                return;
            FrsHost.AddSearchPath(Path.Combine(pulsar, "Legacy", "Local"));
            FrsHost.AddSearchPath(Path.Combine(pulsar, "Interim", "Local"));
            FrsHost.AddSearchPath(Path.Combine(pulsar, "Local"));
        }
        catch
        {
            // ignored
        }
    }
}
