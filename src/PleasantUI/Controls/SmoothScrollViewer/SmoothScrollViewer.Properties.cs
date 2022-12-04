using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

public sealed partial class SmoothScrollViewer
{
    internal const double DefaultSmallChange = 16;

    private IDisposable? _childSubscription;
    private ILogicalScrollable? _logicalScrollable;
    private Size _extent;
    private Vector _offset;
    private Size _viewport;
    private Size _oldExtent;
    private Vector _oldOffset;
    private Size _oldViewport;
    private Size _largeChange;
    private Size _smallChange = new(DefaultSmallChange, DefaultSmallChange);
    private bool _isExpanded;
    private IDisposable? _scrollBarExpandSubscription;

    /// <summary>
    /// Defines the <see cref="CanHorizontallyScroll"/> property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, bool> CanHorizontallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(CanHorizontallyScroll),
            o => o.CanHorizontallyScroll);

    /// <summary>
    /// Defines the <see cref="CanVerticallyScroll"/> property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, bool> CanVerticallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(CanVerticallyScroll),
            o => o.CanVerticallyScroll);

    /// <summary>
    /// Defines the <see cref="Extent"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> ExtentProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(nameof(Extent),
            o => o.Extent,
            (o, v) => o.Extent = v);

    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Vector> OffsetProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Vector>(
            nameof(Offset),
            o => o.Offset,
            (o, v) => o.Offset = v);

    /// <summary>
    /// Defines the <see cref="SmoothScrollEasing"/> property.
    /// </summary>
    public static readonly StyledProperty<Easing?> SmoothScrollEasingProperty =
        AvaloniaProperty.Register<SmoothScrollViewer, Easing?>(nameof(SmoothScrollEasing));

    /// <summary>
    /// Defines the <see cref="SmoothScrollDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> SmoothScrollDurationProperty =
        AvaloniaProperty.Register<SmoothScrollViewer, TimeSpan>(nameof(SmoothScrollDuration));

    /// <summary>
    /// Defines the <see cref="Viewport"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> ViewportProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(nameof(Viewport),
            o => o.Viewport,
            (o, v) => o.Viewport = v);

    /// <summary>
    /// Defines the <see cref="LargeChange"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> LargeChangeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(
            nameof(LargeChange),
            o => o.LargeChange);

    /// <summary>
    /// Defines the <see cref="SmallChange"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, Size> SmallChangeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, Size>(
            nameof(SmallChange),
            o => o.SmallChange);

    /// <summary>
    /// Defines the HorizontalScrollBarMaximum property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> HorizontalScrollBarMaximumProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(HorizontalScrollBarMaximum),
            o => o.HorizontalScrollBarMaximum);

    /// <summary>
    /// Defines the HorizontalScrollBarValue property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> HorizontalScrollBarValueProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(HorizontalScrollBarValue),
            o => o.HorizontalScrollBarValue,
            (o, v) => o.HorizontalScrollBarValue = v);

    /// <summary>
    /// Defines the HorizontalScrollBarViewportSize property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> HorizontalScrollBarViewportSizeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(HorizontalScrollBarViewportSize),
            o => o.HorizontalScrollBarViewportSize);

    /// <summary>
    /// Defines the <see cref="HorizontalScrollBarVisibility"/> property.
    /// </summary>
    public static readonly AttachedProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollViewer, Control, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility));

    /// <summary>
    /// Defines the VerticalScrollBarMaximum property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> VerticalScrollBarMaximumProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(VerticalScrollBarMaximum),
            o => o.VerticalScrollBarMaximum);

    /// <summary>
    /// Defines the VerticalScrollBarValue property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> VerticalScrollBarValueProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(VerticalScrollBarValue),
            o => o.VerticalScrollBarValue,
            (o, v) => o.VerticalScrollBarValue = v);

    /// <summary>
    /// Defines the VerticalScrollBarViewportSize property.
    /// </summary>
    /// <remarks>
    /// There is no public C# accessor for this property as it is intended to be bound to by a 
    /// <see cref="ScrollContentPresenter"/> in the control's template.
    /// </remarks>
    public static readonly DirectProperty<SmoothScrollViewer, double> VerticalScrollBarViewportSizeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, double>(
            nameof(VerticalScrollBarViewportSize),
            o => o.VerticalScrollBarViewportSize);

    /// <summary>
    /// Defines the <see cref="VerticalScrollBarVisibility"/> property.
    /// </summary>
    public static readonly AttachedProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollViewer, Control, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    /// <summary>
    /// Defines the <see cref="IsExpandedProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollViewer, bool> IsExpandedProperty =
        ScrollBar.IsExpandedProperty.AddOwner<SmoothScrollViewer>(o => o.IsExpanded);

    /// <summary>
    /// Defines the <see cref="AllowAutoHide"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AllowAutoHideProperty =
        ScrollBar.AllowAutoHideProperty.AddOwner<SmoothScrollViewer>();

    public static readonly DirectProperty<SmoothScrollViewer, bool> VisibleMaximumProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(VisibleMaximum),
            o => o.VisibleMaximum);

    public static readonly DirectProperty<SmoothScrollViewer, bool> HorizontalScrollBarEnableDecreaseProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(HorizontalScrollBarEnableDecrease),
            o => o.HorizontalScrollBarEnableDecrease);

    public static readonly DirectProperty<SmoothScrollViewer, bool> HorizontalScrollBarEnableIncreaseProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollViewer, bool>(
            nameof(HorizontalScrollBarEnableIncrease),
            o => o.HorizontalScrollBarEnableIncrease);

    /// <summary>
    /// Defines the <see cref="ScrollChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<ScrollChangedEventArgs> ScrollChangedEvent =
        RoutedEvent.Register<SmoothScrollViewer, ScrollChangedEventArgs>(
            nameof(ScrollChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Occurs when changes are detected to the scroll position, extent, or viewport size.
    /// </summary>
    public event EventHandler<ScrollChangedEventArgs> ScrollChanged
    {
        add => AddHandler(ScrollChangedEvent, value);
        remove => RemoveHandler(ScrollChangedEvent, value);
    }

    /// <summary>
    /// Gets or sets the current scroll easing function.
    /// </summary>
    public Easing? SmoothScrollEasing
    {
        get => GetValue(SmoothScrollEasingProperty);
        set => SetValue(SmoothScrollEasingProperty, value);
    }

    /// <summary>
    /// Gets or sets the current smooth-scroll duration.
    /// </summary>
    public TimeSpan SmoothScrollDuration
    {
        get => GetValue(SmoothScrollDurationProperty);
        set => SetValue(SmoothScrollDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal scrollbar visibility.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scrollbar visibility.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the viewer can scroll horizontally.
    /// </summary>
    private bool CanHorizontallyScroll => HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled;

    /// <summary>
    /// Gets a value indicating whether the viewer can scroll vertically.
    /// </summary>
    private bool CanVerticallyScroll => VerticalScrollBarVisibility != ScrollBarVisibility.Disabled;

    /// <summary>
    /// Gets the maximum horizontal scrollbar value.
    /// </summary>
    public double HorizontalScrollBarMaximum => Max(_extent.Width - _viewport.Width, 0);

    public bool VisibleMaximum => Math.Round(HorizontalScrollBarMaximum) > 1;

    public bool HorizontalScrollBarEnableDecrease => Offset.X != 0;

    public bool HorizontalScrollBarEnableIncrease => Offset.X != HorizontalScrollBarMaximum;

    /// <summary>
    /// Gets the large (page) change value for the scroll viewer.
    /// </summary>
    public Size LargeChange => _largeChange;

    /// <summary>
    /// Gets the small (line) change value for the scroll viewer.
    /// </summary>
    public Size SmallChange => _smallChange;

    /// <summary>
    /// Gets or sets the horizontal scrollbar value.
    /// </summary>
    private double HorizontalScrollBarValue
    {
        get => _offset.X;
        set
        {
            if (_offset.X != value)
            {
                double old = Offset.X;
                Offset = Offset.WithX(value);
                RaisePropertyChanged(HorizontalScrollBarValueProperty, old, value);
            }
        }
    }

    /// <summary>
    /// Gets the size of the horizontal scrollbar viewport.
    /// </summary>
    private double HorizontalScrollBarViewportSize => _viewport.Width;

    /// <summary>
    /// Gets the maximum vertical scrollbar value.
    /// </summary>
    public double VerticalScrollBarMaximum => Max(_extent.Height - _viewport.Height, 0);

    /// <summary>
    /// Gets or sets the vertical scrollbar value.
    /// </summary>
    private double VerticalScrollBarValue
    {
        get => _offset.Y;
        set
        {
            if (_offset.Y != value)
            {
                double old = Offset.Y;
                Offset = Offset.WithY(value);
                RaisePropertyChanged(VerticalScrollBarValueProperty, old, value);
            }
        }
    }

    /// <summary>
    /// Gets the size of the vertical scrollbar viewport.
    /// </summary>
    private double VerticalScrollBarViewportSize => _viewport.Height;

    /// <summary>
    /// Gets a value that indicates whether any scrollbar is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        private set => SetAndRaise(ScrollBar.IsExpandedProperty, ref _isExpanded, value);
    }

    /// <summary>
    /// Gets a value that indicates whether scrollbars can hide itself when user is not interacting with it.
    /// </summary>
    public bool AllowAutoHide
    {
        get => GetValue(AllowAutoHideProperty);
        set => SetValue(AllowAutoHideProperty, value);
    }

    /// <summary>
    /// Gets the extent of the scrollable content.
    /// </summary>
    public Size Extent
    {
        get => _extent;
        private set
        {
            if (SetAndRaise(ExtentProperty, ref _extent, value))
            {
                CalculatedPropertiesChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current scroll offset.
    /// </summary>
    public Vector Offset
    {
        get => _offset;
        set
        {
            if (SetAndRaise(OffsetProperty, ref _offset, CoerceOffset(Extent, Viewport, value)))
            {
                CalculatedPropertiesChanged();
            }
        }
    }

    /// <summary>
    /// Gets the size of the viewport on the scrollable content.
    /// </summary>
    public Size Viewport
    {
        get => _viewport;
        private set
        {
            if (SetAndRaise(ViewportProperty, ref _viewport, value))
            {
                CalculatedPropertiesChanged();
            }
        }
    }
}