using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using PleasantUI.Media;

namespace PleasantUI;

public enum PleasantThemeMode
{
    None = -1,
    
    Light = 0,
    Dark = 1,
    Mysterious = 2,
    Turquoise = 3,
    Emerald = 4,
        
    Custom
}
    
public class PleasantTheme : AvaloniaObject, IStyle, IResourceProvider
{
    private readonly Uri _baseUri;
    private Styles _pleasantLight = new();
    private Styles _pleasantDark = new();
    private Styles _pleasantMysterious = new();
    private Styles _pleasantEmerald = new();
    private Styles _pleasantTurquoise = new();
    private Styles _sharedStyles = new();
    private bool _isLoading;
    private IStyle? _loaded;

    private bool _isLoadedCustomMode;
        
    public PleasantTheme(Uri baseUri)
    {
        _baseUri = baseUri;
        InitStyles(baseUri);
    }

    public PleasantTheme(IServiceProvider serviceProvider)
    {
        _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
        InitStyles(_baseUri);
    }

    public static readonly StyledProperty<PleasantThemeMode> ModeProperty =
        AvaloniaProperty.Register<PleasantTheme, PleasantThemeMode>(nameof(Mode), PleasantThemeMode.None);

    public static readonly StyledProperty<Theme> CustomModeProperty =
        AvaloniaProperty.Register<PleasantTheme, Theme>(nameof(CustomMode));

    public PleasantThemeMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public Theme? CustomMode
    {
        get => GetValue(CustomModeProperty);
        set => SetValue(CustomModeProperty!, value);
    }
        
    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);
        
        if (Mode == PleasantThemeMode.None) return;

        if (change.Property == ModeProperty)
        {
            switch (Mode)
            {
                case PleasantThemeMode.Light:
                    (Loaded as Styles)![1] = _pleasantLight[0];
                    break;
                case PleasantThemeMode.Dark:
                    (Loaded as Styles)![1] = _pleasantDark[0];
                    break;
                case PleasantThemeMode.Mysterious:
                    (Loaded as Styles)![1] = _pleasantMysterious[0];
                    break;
                case PleasantThemeMode.Emerald:
                    (Loaded as Styles)![1] = _pleasantEmerald[0];
                    break;
                case PleasantThemeMode.Turquoise:
                    (Loaded as Styles)![1] = _pleasantTurquoise[0];
                    break;
                    
                case PleasantThemeMode.Custom:
                    if (CustomMode is { })
                    {
                        (Loaded as Styles)![1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(CustomMode.ToAxaml());
                        _isLoadedCustomMode = true;
                    }
                    break;
                
                default:
                    break;
            }
        }

        if (change.Property == CustomModeProperty 
            && CustomMode is { } 
            && Mode == PleasantThemeMode.Custom
            && !_isLoadedCustomMode)
        {
            (Loaded as Styles)![1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(CustomMode.ToAxaml());
        }

        _isLoadedCustomMode = false;
    }

    public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

    public IReadOnlyList<IStyle> Children => _loaded?.Children ?? Array.Empty<IStyle>();
    public bool TryGetResource(object key, out object? value)
    {
        if (!_isLoading && Loaded is IResourceProvider provider)
            return provider.TryGetResource(key, out value);

        value = null;
        return false;
    }

    public bool HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;
    public void AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);

    public void RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

    public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;
    public event EventHandler OwnerChanged
    {
        add
        {
            if (Loaded is IResourceProvider provider) 
                provider.OwnerChanged += value;
        }
        remove
        {
            if (Loaded is IResourceProvider provider) 
                provider.OwnerChanged -= value;
        }
    }

    public IStyle Loaded
    {
        get
        {
            if (_loaded == null)
            {
                _isLoading = true;

                switch (Mode)
                {
                    case PleasantThemeMode.Light:
                        _loaded = new Styles { _sharedStyles, _pleasantLight[0] };
                        break;
                    case PleasantThemeMode.Dark:
                        _loaded = new Styles { _sharedStyles, _pleasantDark[0] };
                        break;
                    case PleasantThemeMode.Mysterious:
                        _loaded = new Styles { _sharedStyles, _pleasantMysterious[0] };
                        break;
                    case PleasantThemeMode.Emerald:
                        _loaded = new Styles { _sharedStyles, _pleasantEmerald[0] };
                        break;
                    case PleasantThemeMode.Turquoise:
                        _loaded = new Styles { _sharedStyles, _pleasantTurquoise[0] };
                        break;
                        
                    case PleasantThemeMode.Custom:
                        _loaded = new Styles { _sharedStyles, null! };
                        break;
                    
                    default:
                        _loaded = new Styles { null!, null! };
                        break;
                }

                _isLoading = false;
            }

            return _loaded!;
        }
    }

    private void InitStyles(Uri baseUri)
    {
        _sharedStyles = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://PleasantUI/PleasantUI.axaml")
            }
        };
            
        _pleasantLight = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://PleasantUI/Assets/Themes/Light.axaml")
            }
        };

        _pleasantDark = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://PleasantUI/Assets/Themes/Dark.axaml")
            }
        };

        _pleasantMysterious = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://PleasantUI/Assets/Themes/Mysterious.axaml")
            }
        };

        _pleasantEmerald = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://PleasantUI/Assets/Themes/Emerald.axaml")
            }
        };

        _pleasantTurquoise = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://PleasantUI/Assets/Themes/Turquoise.axaml")
            }
        };
    }
}