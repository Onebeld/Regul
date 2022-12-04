using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PleasantUI.Enums;

namespace PleasantUI.Controls;

public partial class PleasantTabView
{
    private object _fallbackContent = new TextBlock
    {
        Text = "Nothing here", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 16
    };

    private double _heightRemainingSpace;
    private double _widthRemainingSpace;
    private Grid? _grid;
    private double _lastSelectIndex = 0;

    public Button? AdderButton;

    /// <summary>
    ///     Defines the <see cref="ClickOnAddingButton" /> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickOnAddingButtonEvent =
        RoutedEvent.Register<PleasantTabView, RoutedEventArgs>(nameof(ClickOnAddingButton),
            RoutingStrategies.Bubble);

    /// <summary>
    ///     Defines the <see cref="FallBackContent" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabView, object> FallBackContentProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabView, object>
        (nameof(FallBackContent),
            o => o.FallBackContent,
            (o, v) => o.FallBackContent = v);

    /// <summary>
    ///     Defines the <see cref="AdderButtonIsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AdderButtonIsVisibleProperty =
        AvaloniaProperty.Register<PleasantTabView, bool>(nameof(AdderButtonIsVisible), true);

    /// <summary>
    ///     Defines the <see cref="MaxWidthOfItemsPresenter" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxWidthOfItemsPresenterProperty =
        AvaloniaProperty.Register<PleasantTabView, double>(nameof(MaxWidthOfItemsPresenter),
            double.PositiveInfinity);

    public static readonly StyledProperty<IBrush> SecondaryBackgroundProperty =
        AvaloniaProperty.Register<PleasantTabView, IBrush>(nameof(SecondaryBackground));

    /// <summary>
    /// </summary>
    public static readonly StyledProperty<Thickness> ItemsMarginProperty =
        AvaloniaProperty.Register<PleasantTabView, Thickness>(nameof(ItemsMargin));

    /// <summary>
    ///     Defines the <see cref="HeightRemainingSpace" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabView, double> HeightRemainingSpaceProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabView, double>(
            nameof(HeightRemainingSpace),
            o => o.HeightRemainingSpace);

    /// <summary>
    ///     Defines the <see cref="WidthRemainingSpace" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabView, double> WidthRemainingSpaceProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabView, double>(
            nameof(WidthRemainingSpace),
            o => o.WidthRemainingSpace);

    /// <summary>
    ///     Defines the <see cref="ReorderableTabs" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ReorderableTabsProperty =
        AvaloniaProperty.Register<PleasantTabView, bool>(nameof(ReorderableTabs), true);

    /// <summary>
    ///     Defines the <see cref="ImmediateDrag" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ImmediateDragProperty =
        AvaloniaProperty.Register<PleasantTabView, bool>(nameof(ImmediateDrag), true);

    public static readonly StyledProperty<TabViewMarginType> MarginTypeProperty =
        AvaloniaProperty.Register<PleasantTabView, TabViewMarginType>(nameof(MarginType), TabViewMarginType.None);

    /// <summary>
    ///     This content is showed when there is no item.
    /// </summary>
    public object FallBackContent
    {
        get => _fallbackContent;
        set => SetAndRaise(FallBackContentProperty, ref _fallbackContent, value);
    }

    /// <summary>
    ///     This property defines if the AdderButton can be visible, the default value is true.
    /// </summary>
    public bool AdderButtonIsVisible
    {
        get => GetValue(AdderButtonIsVisibleProperty);
        set => SetValue(AdderButtonIsVisibleProperty, value);
    }

    /// <summary>
    ///     This property defines what is the maximum width of the ItemsPresenter.
    /// </summary>
    public double MaxWidthOfItemsPresenter
    {
        get => GetValue(MaxWidthOfItemsPresenterProperty);
        set => SetValue(MaxWidthOfItemsPresenterProperty, value);
    }

    /// <summary>
    ///     Gets or Sets the SecondaryBackground.
    /// </summary>
    public IBrush SecondaryBackground
    {
        get => GetValue(SecondaryBackgroundProperty);
        set => SetValue(SecondaryBackgroundProperty, value);
    }

    /// <summary>
    ///     Sets the margin of the itemspresenter
    /// </summary>
    public Thickness ItemsMargin
    {
        get => GetValue(ItemsMarginProperty);
        set => SetValue(ItemsMarginProperty, value);
    }

    /// <summary>
    ///     Gets the space that remains in the top
    /// </summary>
    public double HeightRemainingSpace
    {
        get => _heightRemainingSpace;
        private set => SetAndRaise(HeightRemainingSpaceProperty, ref _heightRemainingSpace, value);
    }

    /// <summary>
    ///     Gets the space that remains in the top.
    /// </summary>
    public double WidthRemainingSpace
    {
        get => _widthRemainingSpace;
        private set => SetAndRaise(WidthRemainingSpaceProperty, ref _widthRemainingSpace, value);
    }

    /// <summary>
    ///     Gets or Sets if the Children-Tabs can be reorganized by dragging.
    /// </summary>
    public bool ReorderableTabs
    {
        get => GetValue(ReorderableTabsProperty);
        set => SetValue(ReorderableTabsProperty, value);
    }

    /// <summary>
    ///     Gets or sets if the DraggableTabsChildren can be dragged Immediate or on PointerReleased only.
    /// </summary>
    public bool ImmediateDrag
    {
        get => GetValue(ImmediateDragProperty);
        set => SetValue(ImmediateDragProperty, value);
    }

    public TabViewMarginType MarginType
    {
        get => GetValue(MarginTypeProperty);
        set => SetValue(MarginTypeProperty, value);
    }

    /// <summary>
    ///     It's raised when the adder button is clicked
    /// </summary>
    public event EventHandler<RoutedEventArgs> ClickOnAddingButton
    {
        add => AddHandler(ClickOnAddingButtonEvent, value);
        remove => RemoveHandler(ClickOnAddingButtonEvent, value);
    }
}