﻿using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Helpers;
using PleasantUI.Other;

namespace PleasantUI;

[DataContract]
public class PleasantUiSettings : ViewModelBase
{
    private readonly IPlatformSettings _platformSettings;

    private bool _enableShadowing = true;
    private bool _useAccentColorFromSystem = true;
    private bool _enableTransparency;
    private bool _enableCustomTitleBar;
    private PleasantThemeMode _themeMode = PleasantThemeMode.System;
    private string? _customThemeModeName;
    private WindowTransparencyLevel _blurMode;
    private double _opacityLevel = 0.8;
    private string _fontName = null!;
    private uint _uIntAccentColor;
    private AvaloniaList<uint> _colorPalette = new();
    private bool _lightnessAccentColorInCustomMode = false;

    private Color _accentColor;
    private Color _accentColorLightSecondary;
    private Color _accentColorLightTertiary;
    private Color _accentColorLightSelected;

    public static PleasantUiSettings Instance { get; private set; } = new();

    public PleasantUiSettings()
    {
        _platformSettings = AvaloniaLocator.Current.GetRequiredService<IPlatformSettings>();
        Setup();
        _platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
    }
    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (UseAccentColorFromSystem) 
            UIntAccentColor = e.AccentColor1.ToUint32();

        RaiseChangingAccentColor();
    }

    private void RaiseChangingAccentColor()
    {
        PlatformThemeVariant themeVariant = _platformSettings.GetColorValues().ThemeVariant;
        
        if (ThemeMode is PleasantThemeMode.Dark or PleasantThemeMode.Mysterious or PleasantThemeMode.Emerald || 
            ThemeMode is PleasantThemeMode.System && themeVariant is PlatformThemeVariant.Dark ||
            ThemeMode is PleasantThemeMode.Custom && LightnessAccentColorInCustomMode && !string.IsNullOrWhiteSpace(CustomThemeModeName))
        {
            AccentColorLightSelected = AccentColor.ChangeColorBrightness(0.75);
            Hsl accentSelected = ColorExtensions.ToHsl(AccentColorLightSelected);
            if (accentSelected.L >= 0.59)
            {
                accentSelected.L *= 0.85;
                AccentColorLightSelected = accentSelected.ToRgb(AccentColorLightSelected.A);
            }
            AccentColorLightSecondary = AccentColor.ChangeColorBrightness(0.6);
            AccentColorLightTertiary = AccentColor.ChangeColorBrightness(0.5);
        }
        else
        {
            AccentColorLightSecondary = AccentColor;
            AccentColorLightSelected = AccentColor.ChangeColorBrightness(0.95);
            AccentColorLightTertiary = AccentColor.ChangeColorBrightness(0.85);
        }
    }

    private void Setup()
    {
#if Windows
        OperatingSystem operatingSystem = Environment.OSVersion;
        Version currentVersion = operatingSystem.Version;
        
        if (currentVersion >= new Version(10, 0, 22000))
        {
            EnableTransparency = true;
            BlurMode = WindowTransparencyLevel.Mica;
            UseAccentColorFromSystem = true;
            EnableCustomTitleBar = true;
        }
        else if (currentVersion >= new Version(10, 0, 10586))
        {
            EnableTransparency = true;
            BlurMode = WindowTransparencyLevel.Blur;
            UseAccentColorFromSystem = true;
            EnableCustomTitleBar = true;
        }
        else
        {
            EnableTransparency = false;
            BlurMode = WindowTransparencyLevel.None;
            UseAccentColorFromSystem = false;
            EnableCustomTitleBar = false;

            ThemeMode = PleasantThemeMode.Light;
        }

#elif OSX
        EnableTransparency = true;
        UseAccentColorFromSystem = false;
        BlurMode = WindowTransparencyLevel.Blur;
        EnableCustomTitleBar = true;
#else
        EnableTransparency = false;
        UseAccentColorFromSystem = false;
        BlurMode = WindowTransparencyLevel.None;
        EnableCustomBar = false;
#endif

        FontName = FontManager.Current.DefaultFontFamilyName;
    }
    
    [DataMember]
    public bool EnableShadowing
    {
        get => _enableShadowing;
        set => RaiseAndSetIfChanged(ref _enableShadowing, value);
    }
    
    [DataMember]
    public bool EnableCustomTitleBar
    {
        get => _enableCustomTitleBar;
        set => RaiseAndSetIfChanged(ref _enableCustomTitleBar, value);
    }
    
    [DataMember]
    public PleasantThemeMode ThemeMode
    {
        get => _themeMode;
        set
        {
            RaiseAndSetIfChanged(ref _themeMode, value);
            RaiseChangingAccentColor();
        }
    }

    [DataMember]
    public string? CustomThemeModeName
    {
        get => _customThemeModeName;
        set => RaiseAndSetIfChanged(ref _customThemeModeName, value);
    }
    
    [DataMember]
    public WindowTransparencyLevel BlurMode
    {
        get => _blurMode;
        set => RaiseAndSetIfChanged(ref _blurMode, value);
    }
    
    [DataMember]
    public bool UseAccentColorFromSystem
    {
        get => _useAccentColorFromSystem;
        set
        {
            RaiseAndSetIfChanged(ref _useAccentColorFromSystem, value);

#if Windows
            if (value)
                UIntAccentColor = _platformSettings.GetColorValues().AccentColor1.ToUint32();
#endif
        }
    }

    [DataMember]
    public bool LightnessAccentColorInCustomMode
    {
        get => _lightnessAccentColorInCustomMode;
        set
        {
            RaiseAndSetIfChanged(ref _lightnessAccentColorInCustomMode, value);
            
            if (ThemeMode is PleasantThemeMode.Custom)
                RaiseChangingAccentColor();
        }
    }

    [DataMember]
    public double OpacityLevel
    {
        get => _opacityLevel;
        set => RaiseAndSetIfChanged(ref _opacityLevel, value);
    }
    
    [DataMember]
    public string FontName
    {
        get => _fontName;
        set => RaiseAndSetIfChanged(ref _fontName, value);
    }
    
    [DataMember]
    public uint UIntAccentColor
    {
        get => _uIntAccentColor;
        set
        {
            RaiseAndSetIfChanged(ref _uIntAccentColor, value);
            AccentColor = Color.FromUInt32(value);
            RaiseChangingAccentColor();
        }
    }

    [DataMember]
    public AvaloniaList<uint> ColorPalette
    {
        get => _colorPalette;
        set => RaiseAndSetIfChanged(ref _colorPalette, value);
    }
    
    [DataMember]
    public bool EnableTransparency
    {
        get => _enableTransparency;
        set => RaiseAndSetIfChanged(ref _enableTransparency, value);
    }

    #region JsonIgnored

    [JsonIgnore]
    public Color AccentColor
    {
        get => _accentColor;
        private set => RaiseAndSetIfChanged(ref _accentColor, value);
    }

    [JsonIgnore]
    public Color AccentColorLightSecondary
    {
        get => _accentColorLightSecondary;
        private set => RaiseAndSetIfChanged(ref _accentColorLightSecondary, value);
    }

    [JsonIgnore]
    public Color AccentColorLightTertiary
    {
        get => _accentColorLightTertiary;
        private set => RaiseAndSetIfChanged(ref _accentColorLightTertiary, value);
    }

    [JsonIgnore]
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
            Instance = JsonSerializer.Deserialize<PleasantUiSettings>(stream)!;
        }
        catch { }
    }

    public static void Save()
    {
        using FileStream fileStream = File.Create(Path.Combine(Files.Settings));
        JsonSerializer.Serialize(fileStream, Instance);
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