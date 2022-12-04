using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PleasantUI.Controls;

public partial class NavigationViewItemBase
{
    private Geometry? _icon;
    private Type? _typeContent;
    private object _title = "Title";
    private int _navigationViewDistance;
    private double _externalLength;

    public static readonly DirectProperty<NavigationViewItemBase, object> ContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, object>(
            nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);

    public static readonly DirectProperty<NavigationViewItemBase, Type?> TypeContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, Type?>(
            nameof(TypeContent),
            o => o.TypeContent,
            (o, v) => o.TypeContent = v);

    public static readonly DirectProperty<NavigationViewItemBase, Geometry?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, Geometry?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);

    public static readonly DirectProperty<NavigationViewItemBase, object> TitleProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, object>(
            nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, bool>(nameof(IsOpen), true);

    public static readonly StyledProperty<bool> SelectOnCloseProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, bool>(nameof(SelectOnClose));

    public static readonly StyledProperty<ClickMode> ClickModeProperty =
        Button.ClickModeProperty.AddOwner<NavigationViewItemBase>();

    public static readonly DirectProperty<NavigationViewItemBase, int> NavigationViewDistanceProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, int>(nameof(NavigationViewDistance), o => o.Level);

    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, double>(nameof(CompactPaneLength));

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationViewItemBase, double>(nameof(OpenPaneLength));

    public static readonly DirectProperty<NavigationViewItemBase, double> ExternalLengthProperty =
        AvaloniaProperty.RegisterDirect<NavigationViewItemBase, double>(nameof(ExternalLength),
            o => o.ExternalLength);

    public Type? TypeContent
    {
        get => _typeContent;
        set => SetAndRaise(TypeContentProperty, ref _typeContent, value);
    }

    public object Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }

    public Geometry? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    public object Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool SelectOnClose
    {
        get => GetValue(SelectOnCloseProperty);
        set => SetValue(SelectOnCloseProperty, value);
    }

    public ClickMode ClickMode
    {
        get => GetValue(ClickModeProperty);
        set => SetValue(ClickModeProperty, value);
    }

    public int NavigationViewDistance
    {
        get => _navigationViewDistance;
        protected set => SetAndRaise(LevelProperty, ref _navigationViewDistance, value);
    }

    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public double ExternalLength
    {
        get => _externalLength;
        private set => SetAndRaise(ExternalLengthProperty, ref _externalLength, value);
    }
}