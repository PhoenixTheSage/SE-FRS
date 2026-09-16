using ClientPlugin.Frs;
using ClientPlugin.Settings;

namespace ClientPlugin.RichHud;

/// <summary>
/// Shared get/set/save for Pulsar and Rich HUD controls.
/// </summary>
internal static class RichHudOptions
{
    public static AntiAliasingChoice GetAntiAliasing() => Config.Current.AntiAliasing;

    public static void SetAntiAliasing(AntiAliasingChoice value)
    {
        Config.Current.AntiAliasing = value;
        Save();
    }

    public static FrsMode GetMode() => Config.Current.Mode;

    public static void SetMode(FrsMode value)
    {
        Config.Current.Mode = value;
        Save();
    }

    public static float GetSharpness() => Config.Current.Sharpness;

    public static void SetSharpness(float value)
    {
        Config.Current.Sharpness = value;
        Save();
    }

    public static void ShowStatus()
    {
        Config.ShowStatus();
    }

    public static void Save()
    {
        ConfigStorage.Save(Config.Current);
    }
}
