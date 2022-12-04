using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Media;

namespace PleasantUI;

public class PleasantTheme : AvaloniaObject, IStyle, IResourceProvider
{
    private StyleInclude _styleCommon = null!;
    private StyleInclude _styleControls = null!;
    private StyleInclude _pleasantLight = null!;
    private StyleInclude _pleasantDark = null!;
    private StyleInclude _pleasantMysterious = null!;
    private StyleInclude _pleasantEmerald = null!;
    private StyleInclude _pleasantTurquoise = null!;
    private bool _isLoading;
    private IStyle? _loaded;

    public PleasantTheme(IServiceProvider serviceProvider)
    {
        Uri baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))!).BaseUri;
        InitStyles(baseUri);
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
        set => SetValue(CustomThemeProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ModeProperty || change.Property == CustomThemeProperty)
            EnsureTheme();
    }

    public IStyle Loaded
    {
        get
        {
            if (_loaded is null)
            {
                _isLoading = true;

                StyleInclude styleInclude = Mode switch
                {
                    PleasantThemeMode.Dark => _pleasantDark,
                    PleasantThemeMode.Mysterious => _pleasantMysterious,
                    PleasantThemeMode.Emerald => _pleasantEmerald,
                    PleasantThemeMode.Turquoise => _pleasantTurquoise,

                    PleasantThemeMode.Light => _pleasantLight,
                    PleasantThemeMode.Custom => _pleasantLight,
                    _ => throw new ArgumentOutOfRangeException()
                };

                _loaded = new Styles
                {
                    _styleCommon, _styleControls, styleInclude
                };

                _isLoading = false;
            }

            return _loaded!;
        }
    }

    public Theme GetTheme(bool makeClone = false)
    {
        if (Mode != PleasantThemeMode.Custom)
        {
            Theme theme = new()
            {
                Name = Mode.ToString()
            };

            IStyle? style = (Loaded as Styles)!.ElementAtOrDefault(2);

            theme.Colors = ((Style)((StyleInclude)style!).Loaded).ToColorDictionary();

            return theme;
        }
        if (CustomTheme is not null)
            return makeClone ? (Theme)CustomTheme.Clone() : CustomTheme;

        return new Theme
        {
            Name = "Default", Colors = ((Style)_pleasantLight.Loaded).ToColorDictionary()
        };
    }

    public AvaloniaDictionary<string, uint> GetColorDictionaryFromDefaultTheme() => ((Style)_pleasantLight.Loaded).ToColorDictionary();

    public (Theme, bool) CompareWithDefaultTheme(Theme theme)
    {
        bool colorsChanged = false;

        Theme newTheme = GetTheme(true);
        newTheme.Name = theme.Name;

        foreach (KeyValuePair<string, uint> color in newTheme.Colors)
        {
            if (theme.Colors.TryGetValue(color.Key, out uint value))
            {
                newTheme.Colors[color.Key] = value;
                colorsChanged = true;
            }
        }

        return (newTheme, colorsChanged);
    }

    public bool TryGetResource(object key, out object? value)
    {
        if (!_isLoading && Loaded is IResourceProvider provider)
            return provider.TryGetResource(key, out value);

        value = null;
        return false;
    }

    public SelectorMatchResult TryAttach(IStyleable target, object? host) => Loaded.TryAttach(target, host);

    public IReadOnlyList<IStyle> Children => _loaded?.Children ?? Array.Empty<IStyle>();

    public void UpdateCustomTheme() => RaisePropertyChanged(CustomThemeProperty, CustomTheme, CustomTheme);

    public bool HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

    public void AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);

    public void RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

    public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

    public event EventHandler? OwnerChanged
    {
        add
        {
            if (Loaded is IResourceProvider resourceProvider)
                resourceProvider.OwnerChanged += value;
        }
        remove
        {
            if (Loaded is IResourceProvider resourceProvider)
                resourceProvider.OwnerChanged -= value;
        }
    }

    private void EnsureTheme()
    {
        if (DisableUpdateTheme) return;

        IStyle style = Mode switch
        {
            PleasantThemeMode.Dark => _pleasantDark,
            PleasantThemeMode.Mysterious => _pleasantMysterious,
            PleasantThemeMode.Emerald => _pleasantEmerald,
            PleasantThemeMode.Turquoise => _pleasantTurquoise,

            PleasantThemeMode.Light => _pleasantLight,
            PleasantThemeMode.Custom => _pleasantLight,

            _ => throw new ArgumentOutOfRangeException()
        };

        if (Mode == PleasantThemeMode.Custom && CustomTheme is not null)
            style = (IStyle)AvaloniaRuntimeXamlLoader.Load(CustomTheme.ToAxaml());

        (Loaded as Styles)![2] = style;
    }

    private void InitStyles(Uri baseUri)
    {
        PleasantUiSettings.Load();

#if Windows
        if (PleasantUiSettings.Instance.UseAccentColorFromSystem)
            PleasantUiSettings.Instance.UIntAccentColor = ColorExtensions.GetWindowsAccentColor();
#endif

        _styleCommon = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/PleasantTheme.Common.axaml")
        };

        _styleControls = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/PleasantTheme.Controls.axaml")
        };

        _pleasantLight = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/Modes/Light.axaml")
        };

        _pleasantDark = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/Modes/Dark.axaml")
        };

        _pleasantMysterious = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/Modes/Mysterious.axaml")
        };

        _pleasantEmerald = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/Modes/Emerald.axaml")
        };

        _pleasantTurquoise = new StyleInclude(baseUri)
        {
            Source = new Uri("avares://PleasantUI/Modes/Turquoise.axaml")
        };
    }
}