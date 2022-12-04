using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

public partial class PleasantTabItem
{
    private bool _isClosing;
    private Button? _closeButton;

    public static readonly RoutedEvent<RoutedEventArgs> ClosingEvent =
        RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> CloseButtonClickEvent =
        RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(CloseButtonClick), RoutingStrategies.Bubble);

    /// <summary>
    ///     Defines the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsClosable), true);

    /// <summary>
    ///     Defines the <see cref="IsClosing" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabItem, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabItem, bool>(nameof(IsClosing), o => o.IsClosing);

    /// <summary>
    ///     Defines the <see cref="CanBeDragged" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanBeDraggedProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(CanBeDragged), true);

    public static readonly StyledProperty<bool> IsEditedIndicatorProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsEditedIndicator));

    /// <summary>
    ///     This property sets if the PleasantTabItem can be closed
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    ///     Returns if the tab is closing.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    public bool CanBeDragged
    {
        get => GetValue(CanBeDraggedProperty);
        set => SetValue(CanBeDraggedProperty, value);
    }

    public bool IsEditedIndicator
    {
        get => GetValue(IsEditedIndicatorProperty);
        set => SetValue(IsEditedIndicatorProperty, value);
    }

    public event EventHandler<RoutedEventArgs> Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public event EventHandler<RoutedEventArgs> CloseButtonClick
    {
        add => AddHandler(CloseButtonClickEvent, value);
        remove => RemoveHandler(CloseButtonClickEvent, value);
    }
}