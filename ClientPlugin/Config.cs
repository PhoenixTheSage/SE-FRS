using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ClientPlugin.Frs;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using Sandbox.Graphics.GUI;

namespace ClientPlugin;

public class Config : INotifyPropertyChanged
{
    #region Options

    private AntiAliasingChoice antiAliasing = AntiAliasingChoice.Off;
    private FrsMode mode = FrsMode.Quality;
    private float sharpness = 0.5f;

    #endregion

    #region User interface

    public readonly string Title = "FRS";

    internal static bool SuppressApply;

    [Separator("Anti-aliasing")]

    [Dropdown(visibleRows: 10, label: "Anti-aliasing",
        description: "Shared with Options → Graphics and DLSS when that plugin is loaded. Only one upscaler can be selected.")]
    public AntiAliasingChoice AntiAliasing
    {
        get => antiAliasing;
        set
        {
            if (value == AntiAliasingChoice.FRS && GpuSupport.Probed && !GpuSupport.CanOfferFrs)
                value = AntiAliasingChoice.Off;
            SetField(ref antiAliasing, value);
        }
    }

    [XmlIgnore]
    public bool Enabled => antiAliasing == AntiAliasingChoice.FRS;

    // Old configs stored <Enabled>true</Enabled>. XmlSerializer still calls this setter.
    [XmlElement("Enabled")]
    [Browsable(false)]
    public bool EnabledCompat
    {
        get => Enabled;
        set
        {
            if (value)
            {
                if (antiAliasing == AntiAliasingChoice.Off)
                    AntiAliasing = AntiAliasingChoice.FRS;
            }
            else if (antiAliasing == AntiAliasingChoice.FRS)
                AntiAliasing = AntiAliasingChoice.Off;
        }
    }

    [Separator("FRS Super Resolution")]

    [Dropdown(
        description: "Quality trades internal resolution against image quality. NativeAA stays at native resolution.")]
    public FrsMode Mode
    {
        get => mode;
        set => SetField(ref mode, value);
    }

    [Slider(0f, 1f, 0.05f, label: "Sharpness",
        description: "RCAS sharpening applied after upscale. 0 is off.")]
    public float Sharpness
    {
        get => sharpness;
        set => SetField(ref sharpness, value);
    }

    [Separator("Status")]

    [Button(label: "Show Status", description: "GPU, FRS support, resolutions, and Anomaly buffer status")]
    // ReSharper disable once UnusedMember.Global
    public static void ShowStatus()
    {
        GpuSupport.TryProbe();
        MyGuiSandbox.AddScreen(new StatusScreen(FrsStatus.CurrentText));
    }

    #endregion

    #region Property change notification boilerplate

    // Property notifications can run while Current is still being initialized.
    public static readonly Config Default = new();
    public static readonly Config Current = ConfigStorage.Load();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(AntiAliasing) || propertyName == nameof(Mode) ||
            propertyName == nameof(Sharpness))
        {
            DebugLog.Write("config " + propertyName + " aa=" + antiAliasing + " mode=" + mode +
                           " sharpness=" + sharpness);
            FrsRuntime.NotifyConfigChanged();
        }
        if (propertyName == nameof(AntiAliasing) && !SuppressApply)
            GameAntiAliasing.ApplyFromConfig();
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }

    #endregion
}
