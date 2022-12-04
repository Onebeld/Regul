using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Enums;
using PleasantUI.EventArgs;

namespace PleasantUI.Controls;

public partial class PleasantWindow
{
    private Panel _modalWindows = null!;

    public PleasantTitleBar? TitleBar;

    public static readonly StyledProperty<bool> EnableTransparencyProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableTransparency));
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableCustomTitleBar), true);
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<PleasantWindow, string>(nameof(Description));
    public static readonly StyledProperty<Geometry?> LogoGeometryProperty =
        AvaloniaProperty.Register<PleasantWindow, Geometry?>(nameof(LogoGeometry));
    public static readonly StyledProperty<IImage?> ImageIconProperty =
        AvaloniaProperty.Register<PleasantWindow, IImage?>(nameof(ImageIcon));
    public static readonly StyledProperty<TitleBarType> TitleBarTypeProperty =
        AvaloniaProperty.Register<PleasantWindow, TitleBarType>(nameof(TitleBarType));
    public static readonly StyledProperty<WindowButtons> WindowButtonsProperty =
        AvaloniaProperty.Register<PleasantWindow, WindowButtons>(nameof(WindowButtons), WindowButtons.All);

    public static readonly RoutedEvent<TitleBarTypeChangedEventArgs> TitleBarTypeChangedEvent =
        RoutedEvent.Register<PleasantWindow, TitleBarTypeChangedEventArgs>(nameof(TitleBarTypeChanged), RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

    public bool EnableTransparency
    {
        get => GetValue(EnableTransparencyProperty);
        set => SetValue(EnableTransparencyProperty, value);
    }

    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }

    public TitleBarType TitleBarType
    {
        get => GetValue(TitleBarTypeProperty);
        set => SetValue(TitleBarTypeProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    ///     Required if the logo is drawn in vector graphics
    /// </summary>
    public Geometry? LogoGeometry
    {
        get => GetValue(LogoGeometryProperty);
        set => SetValue(LogoGeometryProperty, value);
    }

    public IImage? ImageIcon
    {
        get => GetValue(ImageIconProperty);
        set => SetValue(ImageIconProperty, value);
    }

    public event EventHandler<TitleBarTypeChangedEventArgs>? TitleBarTypeChanged
    {
        add => AddHandler(TitleBarTypeChangedEvent, value);
        remove => RemoveHandler(TitleBarTypeChangedEvent, value);
    }

    /// <summary>
    ///     Shows or hides the Expand and Collapse buttons
    /// </summary>
    public WindowButtons WindowButtons
    {
        get => GetValue(WindowButtonsProperty);
        set => SetValue(WindowButtonsProperty, value);
    }
}