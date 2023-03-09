using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Media;
using PleasantUI.Other;

namespace PleasantUI;

public class PleasantTheme : Styles
{
    private readonly IPlatformSettings _platformSettings;
    
    private readonly IResourceDictionary _pleasantLight;
    private readonly IResourceDictionary _pleasantDark;
    private readonly IResourceDictionary _pleasantMysterious;
    private readonly IResourceDictionary _pleasantEmerald;
    private readonly IResourceDictionary _pleasantTurquoise;

    public PleasantTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
        
        PleasantUiSettings.Load();

        _platformSettings = AvaloniaLocator.Current.GetRequiredService<IPlatformSettings>();
        _platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
        
#if Windows
        if (PleasantUiSettings.Instance.UseAccentColorFromSystem)
            PleasantUiSettings.Instance.UIntAccentColor = _platformSettings.GetColorValues().AccentColor1.ToUint32();
#endif

        _pleasantLight = (IResourceDictionary)GetAndRemove("PleasantLight");
        _pleasantDark = (IResourceDictionary)GetAndRemove("PleasantDark");
        _pleasantMysterious = (IResourceDictionary)GetAndRemove("PleasantMysterious");
        _pleasantEmerald = (IResourceDictionary)GetAndRemove("PleasantEmerald");
        _pleasantTurquoise = (IResourceDictionary)GetAndRemove("PleasantTurquoise");
        
        EnsureTheme();
        
        object GetAndRemove(string key)
        {
            object val = Resources[key]
                         ?? throw new KeyNotFoundException($"Key {key} was not found in the resources");
            Resources.Remove(key);
            return val;
        }
    }
    
    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (PleasantUiSettings.Instance.ThemeMode == PleasantThemeMode.System)
            EnsureTheme();
    }

    public static readonly StyledProperty<PleasantThemeMode> ModeProperty =
        AvaloniaProperty.Register<PleasantTheme, PleasantThemeMode>(nameof(Mode));

    public static readonly StyledProperty<Theme?> CustomThemeProperty =
        AvaloniaProperty.Register<PleasantTheme, Theme?>(nameof(CustomTheme));

    public bool DisableUpdateTheme { get; set; }

    public PleasantThemeMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public Theme? CustomTheme
    {
        get => GetValue(CustomThemeProperty);
        set
        {
            SetValue(CustomThemeProperty, value);
            PleasantUiSettings.Instance.LightnessAccentColorInCustomMode = value.LightnessAccentColor;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ModeProperty || change.Property == CustomThemeProperty)
            EnsureTheme();
    }

    public Theme GetTheme(bool makeClone = false)
    {
        if (Mode != PleasantThemeMode.Custom)
        {
            Theme theme = new()
            {
                Name = Mode.ToString(),
                Colors = ((IResourceDictionary)Resources.MergedDictionaries[0]).ToColorList(),
                LightnessAccentColor = Mode switch
                {
                    PleasantThemeMode.Dark => true,
                    PleasantThemeMode.Emerald => true,
                    PleasantThemeMode.Mysterious => true,
                    PleasantThemeMode.System => _platformSettings.GetColorValues().ThemeVariant is PlatformThemeVariant.Dark,
                    _ => false
                }
            };

            return theme;
        }
        if (CustomTheme is not null)
            return makeClone ? (Theme)CustomTheme.Clone() : CustomTheme;

        return new Theme
        {
            Name = "Default", Colors = _pleasantLight.ToColorList()
        };
    }
    
    public (Theme, bool) CompareWithDefaultTheme(Theme theme)
    {
        bool colorsChanged = false;

        Theme newTheme = GetTheme(true);
        newTheme.Name = theme.Name;
        newTheme.LightnessAccentColor = theme.LightnessAccentColor;

        foreach (KeyColor color in newTheme.Colors)
        {
            KeyColor? keyColor = theme.Colors.FirstOrDefault(x => x.Key == color.Key);

            if (keyColor is not null)
            {
                color.Value = keyColor.Value;
                colorsChanged = true;
            }

            // if (theme.Colors.TryGetValue(color.Key, out uint value))
            // {
            //     newTheme.Colors[color.Key] = value;
            //     colorsChanged = true;
            // }
        }

        return (newTheme, colorsChanged);
    }

    public void UpdateCustomTheme() => OnPropertyChanged(new AvaloniaPropertyChangedEventArgs<Theme?>(this, CustomThemeProperty, CustomTheme, CustomTheme, BindingPriority.Style));

    private void EnsureTheme()
    {
        if (DisableUpdateTheme) return;

        IResourceDictionary themeVariant;

        if (Mode != PleasantThemeMode.Custom || CustomTheme is null)
        {
            themeVariant = Mode switch
            {
                PleasantThemeMode.Dark => _pleasantDark,
                PleasantThemeMode.Mysterious => _pleasantMysterious,
                PleasantThemeMode.Emerald => _pleasantEmerald,
                PleasantThemeMode.Turquoise => _pleasantTurquoise,

                PleasantThemeMode.Light => _pleasantLight,
                PleasantThemeMode.Custom => _pleasantLight,

                _ => _platformSettings.GetColorValues().ThemeVariant switch
                {
                    PlatformThemeVariant.Light => _pleasantLight,
                    PlatformThemeVariant.Dark => _pleasantDark,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }
        else
            themeVariant = CustomTheme.ToResourceDictionary();

        IList<IResourceProvider> dict = Resources.MergedDictionaries;

        if (dict.Count == 0)
            dict.Add(themeVariant);
        else
            dict[0] = themeVariant;
    }
}