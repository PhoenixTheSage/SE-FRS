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

    public static bool CanOffer() =>
        AnomalyRenderer.AllowGuestWork || Dx11Bridge.Call<bool>("CanOffer");

    public static string GetChoice()
    {
        var config = Config.Current;
        var choice = config == null ? AntiAliasingChoice.Off : config.AntiAliasing;
        if (!AnomalyRenderer.AllowDx11Device)
            return GameAntiAliasing.ChoiceName(GameAntiAliasing.DisplayedChoice(choice));
        var displayed = Dx11Bridge.Call<AntiAliasingChoice>("DisplayedChoice", choice);
        return Dx11Bridge.Call<string>("ChoiceName", displayed) ?? ChoiceName(choice);
    }

    public static void ApplyChoice(string choice)
    {
        if (AnomalyRenderer.AllowDx11Device)
        {
            Dx11Bridge.Call("ApplyFromPeer", choice);
            return;
        }
        if (Config.Current == null)
            return;
        if (AnomalyRenderer.AllowGuestWork) GameAntiAliasing.ApplyFromPeer(choice);
        else Config.Current.AntiAliasing = FromName(choice);
    }

    static string ChoiceName(AntiAliasingChoice choice)
    {
        switch (choice)
        {
            case AntiAliasingChoice.FXAA: return ChoiceFxaa;
            case AntiAliasingChoice.DLSS: return ChoiceDlss;
            case AntiAliasingChoice.FRS: return ChoiceFrs;
            default: return ChoiceOff;
        }
    }

    static AntiAliasingChoice FromName(string name)
    {
        if (name == ChoiceDlss) return AntiAliasingChoice.DLSS;
        if (name == ChoiceFrs) return AntiAliasingChoice.FRS;
        if (name == ChoiceFxaa) return AntiAliasingChoice.FXAA;
        return AntiAliasingChoice.Off;
    }
}
