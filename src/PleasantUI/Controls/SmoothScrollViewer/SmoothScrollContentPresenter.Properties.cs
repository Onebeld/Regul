using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PleasantUI.Controls;

public partial class SmoothScrollContentPresenter
{
    private const double EdgeDetectionTolerance = 0.1;
    private const int LogicalScrollItemSize = 50;

    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private bool _arranging;
    private Size _extent;
    private Vector _offset;
    //private Easing _smoothScrollEasing = new BounceEaseInOut();
    //private TimeSpan _smoothScrollDuration = TimeSpan.FromMilliseconds(0);
    //private bool _usesSmoothScrolling = true;
    private readonly DispatcherTimer _smoothScrollTimer; //Timer(1);
    private bool _smoothScrollTimerStarted;
    private double _animStartOffsetX;
    private double _animStartOffsetY;
    private double _offsetX;
    private double _offsetY;
    private double _animDuration;
    private double _animTimeRemaining;

    private Easing? _currentEasing;
    private IDisposable? _logicalScrollSubscription;
    private Size _viewport;
    private Dictionary<int, Vector>? _activeLogicalGestureScrolls;
    private List<IControl>? _anchorCandidates;
    private IControl? _anchorElement;
    private Rect _anchorElementBounds;
    private bool _isAnchorElementDirty;

    /// <summary>
    /// Defines the <see cref="CanHorizontallyScroll"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, bool> CanHorizontallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollContentPresenter, bool>(
            nameof(CanHorizontallyScroll),
            o => o.CanHorizontallyScroll,
            (o, v) => o.CanHorizontallyScroll = v);

    /// <summary>
    /// Defines the <see cref="CanVerticallyScroll"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, bool> CanVerticallyScrollProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollContentPresenter, bool>(
            nameof(CanVerticallyScroll),
            o => o.CanVerticallyScroll,
            (o, v) => o.CanVerticallyScroll = v);

    /// <summary>
    /// Defines the <see cref="Extent"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, Size> ExtentProperty =
        SmoothScrollViewer.ExtentProperty.AddOwner<SmoothScrollContentPresenter>(
            o => o.Extent,
            (o, v) => o.Extent = v);

    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, Vector> OffsetProperty =
        SmoothScrollViewer.OffsetProperty.AddOwner<SmoothScrollContentPresenter>(
            o => o.Offset,
            (o, v) => o.Offset = v);

    /// <summary>
    /// Defines the <see cref="AnimationOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<Vector> AnimationOffsetProperty =
        AvaloniaProperty.Register<SmoothScrollContentPresenter, Vector>(nameof(AnimationOffset));

    /// <summary>
    /// Defines the <see cref="Viewport"/> property.
    /// </summary>
    public static readonly DirectProperty<SmoothScrollContentPresenter, Size> ViewportProperty =
        SmoothScrollViewer.ViewportProperty.AddOwner<SmoothScrollContentPresenter>(
            o => o.Viewport,
            (o, v) => o.Viewport = v);


    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled horizontally.
    /// </summary>
    public bool CanHorizontallyScroll
    {
        get => _canHorizontallyScroll;
        set => SetAndRaise(CanHorizontallyScrollProperty, ref _canHorizontallyScroll, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the content can be scrolled horizontally.
    /// </summary>
    public bool CanVerticallyScroll
    {
        get => _canVerticallyScroll;
        set => SetAndRaise(CanVerticallyScrollProperty, ref _canVerticallyScroll, value);
    }

    /// <summary>
    /// Gets the extent of the scrollable content.
    /// </summary>
    public Size Extent
    {
        get => _extent;
        private set => SetAndRaise(ExtentProperty, ref _extent, value);
    }

    /// <summary>
    /// Gets or sets the current logical scroll offset.
    /// </summary>
    public Vector Offset
    {
        get => _offset;
        set => SetAndRaise(OffsetProperty, ref _offset, SmoothScrollViewer.CoerceOffset(Extent, Viewport, value));
    }

    /// <summary>
    /// Gets or sets the current visible scroll offset.
    /// </summary>
    public Vector AnimationOffset
    {
        get => GetValue(AnimationOffsetProperty);
        set => SetValue(AnimationOffsetProperty, value);
    }

    /// <summary>
    /// Gets the size of the viewport on the scrollable content.
    /// </summary>
    public Size Viewport
    {
        get => _viewport;
        private set => SetAndRaise(ViewportProperty, ref _viewport, value);
    }

    private bool UsesSmoothScrolling => ShouldUseSmoothScrolling(out Easing _, out TimeSpan __);

    private Vector CurrentFromOffset => UsesSmoothScrolling ? AnimationOffset : Offset;
}