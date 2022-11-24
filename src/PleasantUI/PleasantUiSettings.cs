using System.Runtime.Serialization;
using System.Xml.Serialization;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
#if Windows
using Avalonia.Win32;
#endif
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Other;

namespace PleasantUI;

[DataContract]
public class PleasantUiSettings : ViewModelBase
{
    public const uint DefaultAccentColor = 0xFF176A80;
    
    private bool _enableShadowing = true;
    private bool _useAccentColorFromSystem = true;
    private bool _enableTransparency;
    private bool _enableCustomTitleBar;
    private PleasantThemeMode _themeMode = PleasantThemeMode.Light;
    private string? _customThemeModeName;
    private WindowTransparencyLevel _blurMode;
    private double _opacityLevel = 0.8;
    private string _fontName = null!;
    private uint _uIntAccentColor;
    private AvaloniaList<uint> _colorPalette = new();

    private Color _accentColor;
    private Color _accentColorDarkSecondary;
    private Color _accentColorDarkTertiary;
    private Color _accentColorLightSecondary;
    private Color _accentColorLightTertiary;
    private Color _accentColorLightSelected;
    
    public static PleasantUiSettings Instance = new();

    public PleasantUiSettings() => Setup();

    private void Setup()
    {
#if Windows
        if (Win32Platform.WindowsVersion >= new Version(10, 0, 22000))
        {
            EnableTransparency = true;
            BlurMode = WindowTransparencyLevel.Mica;
            UseAccentColorFromSystem = true;
            EnableCustomTitleBar = true;

            ThemeMode = ThemeExtensions.WindowsThemeIsLight() ? PleasantThemeMode.Light : PleasantThemeMode.Dark;
        }
        else if (Win32Platform.WindowsVersion >= new Version(10, 0, 10586))
        {
            EnableTransparency = true;
            BlurMode = WindowTransparencyLevel.Blur;
            UseAccentColorFromSystem = true;
            EnableCustomTitleBar = true;

            ThemeMode = ThemeExtensions.WindowsThemeIsLight() ? PleasantThemeMode.Light : PleasantThemeMode.Dark;
        }
        else
        {
            EnableTransparency = false;
            BlurMode = WindowTransparencyLevel.None;
            UseAccentColorFromSystem = false;
            EnableCustomTitleBar = false;
        }
        
#elif OSX
        EnableTransparency = true;
        UseAccentColorFromSystem = false;
        BlurMode = WindowTransparencyLevel.Blur;
        AccentColor = Color.FromUInt32(DefaultAccentColor);
        EnableCustomTitleBar = true;
#else
        EnableTransparency = false;
        UseAccentColorFromSystem = false;
        BlurMode = WindowTransparencyLevel.None;
        AccentColor = Color.FromUInt32(DefaultAccentColor);
        EnableCustomBar = false;
#endif
        
        FontName = FontManager.Current.DefaultFontFamilyName;
    }

    [XmlAttribute]
    [DataMember]
    public bool EnableShadowing
    {
        get => _enableShadowing;
        set => RaiseAndSetIfChanged(ref _enableShadowing, value);
    }
    
    [XmlAttribute]
    [DataMember]
    public bool EnableCustomTitleBar
    {
        get => _enableCustomTitleBar;
        set => RaiseAndSetIfChanged(ref _enableCustomTitleBar, value);
    }

    [XmlAttribute]
    [DataMember]
    public PleasantThemeMode ThemeMode
    {
        get => _themeMode;
        set => RaiseAndSetIfChanged(ref _themeMode, value);
    }

    [XmlAttribute]
    [DataMember]
    public string? CustomThemeModeName
    {
        get => _customThemeModeName;
        set => RaiseAndSetIfChanged(ref _customThemeModeName, value);
    }

    [XmlAttribute]
    [DataMember]
    public WindowTransparencyLevel BlurMode
    {
        get => _blurMode;
        set => RaiseAndSetIfChanged(ref _blurMode, value);
    }

    [XmlAttribute]
    [DataMember]
    public bool UseAccentColorFromSystem
    {
        get => _useAccentColorFromSystem;
        set
        {
            RaiseAndSetIfChanged(ref _useAccentColorFromSystem, value);

#if Windows
            if (value) 
                UIntAccentColor = ColorExtensions.GetWindowsAccentColor();
#endif
        }
    }

    [XmlAttribute]
    [DataMember]
    public double OpacityLevel
    {
        get => _opacityLevel;
        set => RaiseAndSetIfChanged(ref _opacityLevel, value);
    }

    [XmlAttribute]
    [DataMember]
    public string FontName
    {
        get => _fontName;
        set => RaiseAndSetIfChanged(ref _fontName, value);
    }

    [XmlAttribute]
    [DataMember]
    public uint UIntAccentColor
    {
        get => _uIntAccentColor;
        set
        {
            RaiseAndSetIfChanged(ref _uIntAccentColor, value);
            AccentColor = Color.FromUInt32(value);
        }
    }

    [DataMember]
    public AvaloniaList<uint> ColorPalette
    {
        get => _colorPalette;
        set => RaiseAndSetIfChanged(ref _colorPalette, value);
    }
    
    [XmlAttribute]
    [DataMember]
    public bool EnableTransparency
    {
        get => _enableTransparency;
        set => RaiseAndSetIfChanged(ref _enableTransparency, value);
    }

    #region XmlIgnored
    
    [XmlIgnore]
    public Color AccentColor
    {
        get => _accentColor;
        private set
        {
            RaiseAndSetIfChanged(ref _accentColor, value);

            AccentColorDarkSecondary = value.ChangeColorBrightness(1.5);
            AccentColorDarkTertiary = value.ChangeColorBrightness(1.7);
            AccentColorLightSelected = value.ChangeColorBrightness(0.75);
            AccentColorLightSecondary = value.ChangeColorBrightness(0.7);
            AccentColorLightTertiary = value.ChangeColorBrightness(0.6);
        }
    }

    [XmlIgnore]
    public Color AccentColorDarkSecondary
    {
        get => _accentColorDarkSecondary;
        private set => RaiseAndSetIfChanged(ref _accentColorDarkSecondary, value);
    }

    [XmlIgnore]
    public Color AccentColorDarkTertiary
    {
        get => _accentColorDarkTertiary;
        private set => RaiseAndSetIfChanged(ref _accentColorDarkTertiary, value);
    }

    [XmlIgnore]
    public Color AccentColorLightSecondary
    {
        get => _accentColorLightSecondary;
        private set => RaiseAndSetIfChanged(ref _accentColorLightSecondary, value);
    }

    [XmlIgnore]
    public Color AccentColorLightTertiary
    {
        get => _accentColorLightTertiary;
        private set => RaiseAndSetIfChanged(ref _accentColorLightTertiary, value);
    }

    [XmlIgnore]
    public Color AccentColorLightSelected
    {
        get => _accentColorLightSelected;
        private set => RaiseAndSetIfChanged(ref _accentColorLightSelected, value);
    }

    #endregion

    public static void Load()
    {
        if (!Directory.Exists(Directories.Settings))
            Directory.CreateDirectory(Directories.Settings);

        if (!File.Exists(Files.Settings)) return;
        
        using FileStream stream = File.OpenRead(Files.Settings);
        try
        {
            Instance = (PleasantUiSettings)new XmlSerializer(typeof(PleasantUiSettings)).Deserialize(stream)!;
        }
        catch { }
    }

    public static void Save()
    {
        using FileStream fileStream = File.Create(Path.Combine(Files.Settings));
        new XmlSerializer(typeof(PleasantUiSettings)).Serialize(fileStream, Instance);
    }

    public static void Reset()
    {
        Instance.Setup();
        Instance.EnableShadowing = true;
        Instance.ColorPalette.Clear();
        Instance.OpacityLevel = 0.8;
        Instance.CustomThemeModeName = null;
    }
}