using System;
using System.Collections.Generic;
using ClientPlugin.Frs;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Graphics.GUI;
using VRageRender;
using Config = ClientPlugin.Config;

static class Program
{
    static int checks;
    static readonly AntiAliasingChoice Own = AntiAliasingChoice.FRS;
    static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
    static MyGuiControlCombobox Combo()
    {
        var combo = new MyGuiControlCombobox(); combo.AddItem(0, "Off"); combo.AddItem(1, "FXAA"); return combo;
    }
    static void Reset(AntiAliasingChoice choice = AntiAliasingChoice.Off)
    {
        GameAntiAliasing.Reset(); Config.SuppressApply = false; Config.Current.Value = choice;
        MyVideoSettingsManager.CurrentGraphicsSettings = default; MyVideoSettingsManager.Applies = 0;
        AntiAliasingHandshake.Offer = true; PeerAntiAliasing.Present = false; PeerAntiAliasing.CanOffer = true;
        PeerAntiAliasing.Choice = "Off"; PeerAntiAliasing.Calls = 0; ClientPlugin.Settings.ConfigStorage.Saves = 0;
    }
    static void Main()
    {
        foreach (bool guest in new[] { true, false })
        {
            AnomalyRenderer.AllowGuestWork = guest; AnomalyRenderer.AllowDx11Device = !guest;
            Reset(); var plugin = Combo();
            GameAntiAliasing.FillAndBindPluginCombo(plugin); GameAntiAliasing.FillAndBindPluginCombo(plugin);
            Check(plugin.Subscribers == 1, "one handler on rebinding");
            plugin.SelectItemByKey((long)Own);
            Check(Config.Current.AntiAliasing == Own, "plugin selection changes saved choice");
            Check(MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.AntialiasingMode == MyAntialiasingMode.NONE, "upscaler maps to native AA off");
            var replacement = Combo(); GameAntiAliasing.FillAndBindPluginCombo(replacement);
            Check(plugin.Subscribers == 0 && replacement.Subscribers == 1, "old control detached");
            GameAntiAliasing.Reset(); Check(replacement.Subscribers == 0, "reset detaches plugin control");

            Reset(Own); var graphics = Combo(); GameAntiAliasing.BindGraphicsCombo(graphics);
            Check(graphics.GetSelectedKey() == 101, "graphics opens at selected upscaler");
            graphics.SelectItemByKey(1);
            Check(Config.Current.AntiAliasing == AntiAliasingChoice.FXAA, "graphics preview");
            Check(MyVideoSettingsManager.Applies == 0, "preview does not commit native settings");
            GameAntiAliasing.OnGraphicsScreenClosed();
            Check(Config.Current.AntiAliasing == Own && graphics.Subscribers == 0, "cancel restores original and detaches");
            Check(ClientPlugin.Settings.ConfigStorage.Saves == 0, "cancel does not persist preview");

            Reset(); graphics = Combo(); GameAntiAliasing.BindGraphicsCombo(graphics); graphics.SelectItemByKey(101);
            GameAntiAliasing.OnGraphicsOk(); GameAntiAliasing.OnGraphicsScreenClosed();
            Check(Config.Current.AntiAliasing == Own && ClientPlugin.Settings.ConfigStorage.Saves == 1, "OK persists selection");
            var settings = default(MyGraphicsSettings); var perf = settings.PerformanceSettings; var render = perf.RenderSettings;
            render.AntialiasingMode = (MyAntialiasingMode)101; perf.RenderSettings = render; settings.PerformanceSettings = perf;
            GameAntiAliasing.RemapFrsKey(graphics, ref settings);
            Check(settings.PerformanceSettings.RenderSettings.AntialiasingMode == MyAntialiasingMode.NONE, "custom key never reaches native enum");

            Reset(); GameAntiAliasing.ApplyFromUi(AntiAliasingChoice.FXAA);
            Check(MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.AntialiasingMode == MyAntialiasingMode.FXAA, "FXAA UI commit applies native setting");
            Reset(); PeerAntiAliasing.Present = true; GameAntiAliasing.ApplyFromUi(Own);
            Check(PeerAntiAliasing.Choice == "FRS" && PeerAntiAliasing.Calls == 1, "UI synchronizes peer");
            Reset(); PeerAntiAliasing.Present = true; GameAntiAliasing.ApplyFromPeer("FRS");
            Check(Config.Current.AntiAliasing == Own && PeerAntiAliasing.Calls == 0, "peer callback does not bounce");
            Reset(); AntiAliasingHandshake.Offer = false; plugin = Combo(); GameAntiAliasing.FillAndBindPluginCombo(plugin);
            Check(plugin.TryGetItemByKey((long)Own) == null && GameAntiAliasing.DisplayedChoice(Own) == AntiAliasingChoice.Off, "unsupported choice omitted");
        }
        Console.WriteLine("PASS " + checks + " production AA controller checks across guest/DX11 routing (game/UI/capability boundaries are stand-ins).");
    }
}

namespace ClientPlugin
{
    static class Config
    {
        public static bool SuppressApply;
        public static readonly Values Current = new Values();
        public sealed class Values
        {
            internal AntiAliasingChoice Value;
            public AntiAliasingChoice AntiAliasing
            {
                get => Value;
                set { Value = value; if (!SuppressApply) GameAntiAliasing.ApplyFromConfig(); }
            }
        }
    }
}
namespace ClientPlugin.Frs
{
    static class AnomalyRenderer { public static bool AllowGuestWork, AllowDx11Device; }
    static class AntiAliasingHandshake
    {
        public const long GraphicsComboKey = 101;
        public const string ChoiceOff = "Off", ChoiceFxaa = "FXAA", ChoiceDlss = "DLSS", ChoiceFrs = "FRS";
        public static bool Offer = true;
        public static bool CanOffer() => Offer;
    }
    static class PeerAntiAliasing
    {
        public static bool Present, CanOffer = true;
        public const long GraphicsComboKey = 100;
        public static string Choice = "Off";
        public static int Calls;
        public static string GetChoice() => Choice;
        public static void ApplyChoice(string name) { Choice = name; Calls++; }
    }
    static class Dx11Bridge
    {
        public static T Call<T>(string method)
        {
            if (!AnomalyRenderer.AllowDx11Device) throw new Exception("DX11 companion touched from guest settings");
            return default;
        }
    }
    static class VulkanStatus
    {
        internal struct Snap { public int Supported, Vendor; }
        public static Snap Read() => default;
    }
    static class DebugLog { public static void Write(string value) { } }
}
namespace ClientPlugin.RichHud { static class AnomalyTerminalHook { public static void TryRefresh() { } } }
namespace ClientPlugin.Settings { static class ConfigStorage { public static int Saves; public static void Save(object value) { Saves++; } } }
namespace VRageRender
{
    enum MyAntialiasingMode { NONE, FXAA }
}
namespace Sandbox.Engine.Platform.VideoMode
{
    struct RenderSettings { public MyAntialiasingMode AntialiasingMode; }
    struct PerformanceSettings { public RenderSettings RenderSettings; }
    struct MyGraphicsSettings { public PerformanceSettings PerformanceSettings; }
    static class MyVideoSettingsManager
    {
        public static MyGraphicsSettings CurrentGraphicsSettings;
        public static int Applies;
        public static void Apply(MyGraphicsSettings value) { CurrentGraphicsSettings = value; Applies++; }
    }
}
namespace Sandbox.Graphics.GUI
{
    sealed class MyGuiControlCombobox
    {
        readonly Dictionary<long, string> items = new Dictionary<long, string>();
        long selected;
        public event Action ItemSelected;
        public int Subscribers => ItemSelected?.GetInvocationList().Length ?? 0;
        public string TryGetItemByKey(long key) => items.TryGetValue(key, out var value) ? value : null;
        public void AddItem(long key, string name) => items.Add(key, name);
        public long GetSelectedKey() => selected;
        public void SelectItemByKey(long key, bool sendEvent = true)
        {
            if (!items.ContainsKey(key)) return;
            selected = key; if (sendEvent) ItemSelected?.Invoke();
        }
    }
}