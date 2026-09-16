namespace ClientPlugin.Frs;

/// <summary>
/// Well-known AA handshake. DLSS resolves this type by name:
/// <c>ClientPlugin.Frs.AntiAliasingHandshake</c>. No compile-time reference.
/// Choice names: Off, FXAA, DLSS, FRS. Graphics combo key 101 is FRS; DLSS uses 100.
/// </summary>
public static class AntiAliasingHandshake
{
    public const string ChoiceOff = "Off";
    public const string ChoiceFxaa = "FXAA";
    public const string ChoiceDlss = "DLSS";
    public const string ChoiceFrs = "FRS";
    public const long GraphicsComboKey = 101;

    public static bool CanOffer() => GpuSupport.CanOfferFrs;

    public static string GetChoice()
    {
        var config = Config.Current;
        var choice = config == null ? AntiAliasingChoice.Off : config.AntiAliasing;
        return GameAntiAliasing.ChoiceName(GameAntiAliasing.DisplayedChoice(choice));
    }

    public static void ApplyChoice(string choice)
    {
        GameAntiAliasing.ApplyFromPeer(choice);
    }
}
