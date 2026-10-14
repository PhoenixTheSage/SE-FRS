using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ClientPlugin.Frs;
using ClientPlugin.RichHud;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using Sandbox.Graphics.GUI;
using VRage.Plugins;
using VRage.Utils;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SpaceEngineersFRS.Dx11")]

#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.6.0.0")]
[assembly: AssemblyFileVersion("1.6.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public sealed class Plugin : IPlugin
{
    public const string Name = "SpaceEngineersFRS";
    public static Plugin Instance { get; private set; }
    internal static readonly List<string> SearchPaths = new List<string>();

    private SettingsGenerator settingsGenerator;
    private bool disposed;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        disposed = false;
        Instance = this;
        settingsGenerator = new SettingsGenerator();
        DebugLog.Open();
        AnomalyTerminalHook.TryInstall();
        if (AnomalyRenderer.Decided && !AnomalyRenderer.AllowDx11Device)
            MyLog.Default.WriteLine(ClosedRoute("FRS"));
    }

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        DebugLog.Write("Dispose");
        PeerAntiAliasing.Reset();
        if (AnomalyRenderer.AllowDx11Device)
            Dx11Bridge.Call("OnDispose");
        ConfigStorage.FlushPending(true);
        AnomalyTerminalHook.Reset();
        settingsGenerator = null;
        if (ReferenceEquals(Instance, this))
            Instance = null;
        DebugLog.Close();
    }

    public void Update()
    {
        if (disposed)
            return;
        AnomalyTerminalHook.TryInstall();
        ConfigStorage.FlushPending();
        RequestRenderer(4, AnomalyRenderer.AllowGuestWork && Config.Current.Enabled);
        if (AnomalyRenderer.AllowGuestWork)
            FsrVulkan.Publish(Config.Current.Enabled, Config.Current.Mode, Config.Current.Sharpness, SearchPaths);
        if (!AnomalyRenderer.AllowDx11Device)
        {
            if (AnomalyRenderer.AllowGuestWork)
                VulkanStatus.Sync(Config.Current.Enabled);
            return;
        }
        Dx11Bridge.Call("OnUpdate");
    }

    public void OpenConfigDialog()
    {
        var generator = settingsGenerator;
        if (disposed || generator == null)
            return;
        if (AnomalyRenderer.AllowDx11Device)
            Dx11Bridge.Call("PrepareConfig");
        generator.SetLayout<Simple>();
        generator.Dialog.RecreateControls(true);
        MyGuiSandbox.AddScreen(generator.Dialog);
    }

    public void LoadAssets(IReadOnlyDictionary<string, string> assets)
    {
        if (disposed || assets == null)
            return;
        foreach (var pair in assets)
            AddAssetSearchPath(pair.Value, pair.Key);
        AnomalyTerminalHook.TryInstall();
    }

    public void LoadAssets(string folder) => AddAssetSearchPath(folder, null);

    static void RequestRenderer(int bit, bool enabled)
    {
        if (!AnomalyRenderer.AllowGuestWork)
            return;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType("ClientPlugin.Shaders.DisplayServices", false);
            if (type == null)
                continue;
            type.GetMethod(enabled ? "Request" : "Release")?.Invoke(null, new object[] { bit });
            return;
        }
    }

    static string ClosedRoute(string name) =>
        AnomalyRenderer.LinuxRequiresGuest
            ? name + ": Linux stays idle until Anomaly's Vulkan guest bus is open"
            : AnomalyRenderer.AllowGuestWork
                ? name + ": Vulkan evaluates FSR 3.1 when amd_fidelityfx_vk.dll is next to the plugin. The renderer temporal upscale stays the fallback."
                : name + ": DX11 device path closed (" + AnomalyRenderer.ActiveName + ")";

    void AddAssetSearchPath(string path, string name)
    {
        if (disposed || string.IsNullOrEmpty(path))
            return;
        if (File.Exists(path))
            path = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(path))
            SearchPaths.Add(path);
        if (AnomalyRenderer.AllowDx11Device)
            Dx11Bridge.Call("AddSearchPath", path);
        var label = string.IsNullOrEmpty(name) ? path : name + "=" + path;
        MyLog.Default.WriteLine("FRS asset: " + label);
        DebugLog.Write("LoadAssets " + label);
    }
}
