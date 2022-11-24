using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

public sealed partial class SmoothScrollViewer : ContentControl, IScrollable, IScrollAnchorProvider
{
    /// <summary>
    /// Initializes static members of the <see cref="SmoothScrollViewer"/> class.
    /// </summary>
    static SmoothScrollViewer()
    {
        HorizontalScrollBarVisibilityProperty.Changed.AddClassHandler<SmoothScrollViewer>((x, e) =>
            x.ScrollBarVisibilityChanged(e));
        VerticalScrollBarVisibilityProperty.Changed.AddClassHandler<SmoothScrollViewer>((x, e) =>
            x.ScrollBarVisibilityChanged(e));

        AffectsMeasure<SmoothScrollViewer>(SmoothScrollEasingProperty, SmoothScrollDurationProperty);
        AffectsArrange<SmoothScrollViewer>(SmoothScrollEasingProperty, SmoothScrollDurationProperty);
        AffectsRender<SmoothScrollViewer>(SmoothScrollEasingProperty, SmoothScrollDurationProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmoothScrollViewer"/> class.
    /// </summary>
    public SmoothScrollViewer()
    {
        LayoutUpdated += OnLayoutUpdated;
    }

    /// <summary>
    /// Scrolls the content up one line.
    /// </summary>
    public void LineUp()
    {
        Offset -= new Vector(0, _smallChange.Height);
    }

    /// <summary>
    /// Scrolls the content down one line.
    /// </summary>
    public void LineDown()
    {
        Offset += new Vector(0, _smallChange.Height);
    }

    /// <summary>
    /// Scrolls the content left one line.
    /// </summary>
    public void LineLeft()
    {
        Offset -= new Vector(_smallChange.Width, 0);
    }

    /// <summary>
    /// Scrolls the content right one line.
    /// </summary>
    public void LineRight()
    {
        Offset += new Vector(_smallChange.Width, 0);
    }

    /// <summary>
    /// Scrolls the content upward by one page.
    /// </summary>
    public void PageUp()
    {
        VerticalScrollBarValue = Math.Max(_offset.Y - _viewport.Height, 0);
    }

    /// <summary>
    /// Scrolls the content downward by one page.
    /// </summary>
    public void PageDown()
    {
        VerticalScrollBarValue = Math.Min(_offset.Y + _viewport.Height, VerticalScrollBarMaximum);
    }

    /// <summary>
    /// Scrolls the content left by one page.
    /// </summary>
    public void PageLeft()
    {
        HorizontalScrollBarValue = Math.Max(_offset.X - _viewport.Width, 0);
    }

    /// <summary>
    /// Scrolls the content tight by one page.
    /// </summary>
    public void PageRight()
    {
        HorizontalScrollBarValue = Math.Min(_offset.X + _viewport.Width, HorizontalScrollBarMaximum);
    }

    /// <summary>
    /// Scrolls to the top-left corner of the content.
    /// </summary>
    public void ScrollToHome()
    {
        Offset = new Vector(double.NegativeInfinity, double.NegativeInfinity);
    }

    /// <summary>
    /// Scrolls to the bottom-left corner of the content.
    /// </summary>
    public void ScrollToEnd()
    {
        Offset = new Vector(double.NegativeInfinity, double.PositiveInfinity);
    }

    /// <summary>
    /// Gets the value of the HorizontalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to read the value from.</param>
    /// <returns>The value of the property.</returns>
    public static ScrollBarVisibility GetHorizontalScrollBarVisibility(Control control)
    {
        return control.GetValue(HorizontalScrollBarVisibilityProperty);
    }

    /// <summary>
    /// Gets the value of the HorizontalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to set the value on.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetHorizontalScrollBarVisibility(Control control, ScrollBarVisibility value)
    {
        control.SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets the value of the VerticalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to read the value from.</param>
    /// <returns>The value of the property.</returns>
    public static ScrollBarVisibility GetVerticalScrollBarVisibility(Control control)
    {
        return control.GetValue(VerticalScrollBarVisibilityProperty);
    }

    /// <summary>
    /// Gets the value of the VerticalScrollBarVisibility attached property.
    /// </summary>
    /// <param name="control">The control to set the value on.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetVerticalScrollBarVisibility(Control control, ScrollBarVisibility value)
    {
        control.SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <inheritdoc/>
    public void RegisterAnchorCandidate(IControl element)
    {
        (Presenter as IScrollAnchorProvider)?.RegisterAnchorCandidate(element);
    }

    /// <inheritdoc/>
    public void UnregisterAnchorCandidate(IControl element)
    {
        (Presenter as IScrollAnchorProvider)?.UnregisterAnchorCandidate(element);
    }

    /// <inheritdoc/>
    public IControl? CurrentAnchor => (Presenter as IScrollAnchorProvider)?.CurrentAnchor;

    protected override bool RegisterContentPresenter(IContentPresenter presenter)
    {
        _childSubscription?.Dispose();
        _childSubscription = null;

        if (base.RegisterContentPresenter(presenter))
        {
            _childSubscription = Presenter?
                .GetObservable(ContentPresenter.ChildProperty)
                .Subscribe(ChildChanged);
            return true;
        }

        return false;
    }

    internal static Vector CoerceOffset(Size extent, Size viewport, Vector offset)
    {
        double maxX = Math.Max(extent.Width - viewport.Width, 0);
        double maxY = Math.Max(extent.Height - viewport.Height, 0);
        return new Vector(Clamp(offset.X, 0, maxX), Clamp(offset.Y, 0, maxY));
    }

    private static double Clamp(double value, double min, double max)
    {
        return value < min ? min : value > max ? max : value;
    }

    private static double Max(double x, double y)
    {
        double result = Math.Max(x, y);
        return double.IsNaN(result) ? 0 : result;
    }

    private void ChildChanged(IControl? child)
    {
        if (_logicalScrollable != null)
        {
            _logicalScrollable.ScrollInvalidated -= LogicalScrollInvalidated;
            _logicalScrollable = null;
        }

        if (child is ILogicalScrollable logical)
        {
            _logicalScrollable = logical;
            logical.ScrollInvalidated += LogicalScrollInvalidated;
        }

        CalculatedPropertiesChanged();
    }

    private void LogicalScrollInvalidated(object? sender, System.EventArgs e)
    {
        CalculatedPropertiesChanged();
    }

    private void ScrollBarVisibilityChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool wasEnabled = !ScrollBarVisibility.Disabled.Equals(e.OldValue);
        bool isEnabled = !ScrollBarVisibility.Disabled.Equals(e.NewValue);

        if (wasEnabled != isEnabled)
        {
            if (e.Property == HorizontalScrollBarVisibilityProperty)
            {
                RaisePropertyChanged(
                    CanHorizontallyScrollProperty,
                    wasEnabled,
                    isEnabled);
            }
            else if (e.Property == VerticalScrollBarVisibilityProperty)
            {
                RaisePropertyChanged(
                    CanVerticallyScrollProperty,
                    wasEnabled,
                    isEnabled);
            }
        }
    }

    private void CalculatedPropertiesChanged()
    {
        // Pass old values of 0 here because we don't have the old values at this point,
        // and it shouldn't matter as only the template uses these properies.
        RaisePropertyChanged(HorizontalScrollBarMaximumProperty, 0, HorizontalScrollBarMaximum);
        RaisePropertyChanged(HorizontalScrollBarValueProperty, 0, HorizontalScrollBarValue);
        RaisePropertyChanged(HorizontalScrollBarViewportSizeProperty, 0, HorizontalScrollBarViewportSize);
        RaisePropertyChanged(VerticalScrollBarMaximumProperty, 0, VerticalScrollBarMaximum);
        RaisePropertyChanged(VerticalScrollBarValueProperty, 0, VerticalScrollBarValue);
        RaisePropertyChanged(VerticalScrollBarViewportSizeProperty, 0, VerticalScrollBarViewportSize);
        RaisePropertyChanged(VisibleMaximumProperty, false, VisibleMaximum);
        RaisePropertyChanged(HorizontalScrollBarEnableIncreaseProperty, false, HorizontalScrollBarEnableIncrease);
        RaisePropertyChanged(HorizontalScrollBarEnableDecreaseProperty, false, HorizontalScrollBarEnableDecrease);

        if (_logicalScrollable?.IsLogicalScrollEnabled == true)
        {
            SetAndRaise(SmallChangeProperty, ref _smallChange, _logicalScrollable.ScrollSize);
            SetAndRaise(LargeChangeProperty, ref _largeChange, _logicalScrollable.PageScrollSize);
        }
        else
        {
            SetAndRaise(SmallChangeProperty, ref _smallChange, new Size(DefaultSmallChange, DefaultSmallChange));
            SetAndRaise(LargeChangeProperty, ref _largeChange, Viewport);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.PageUp)
        {
            PageUp();
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown)
        {
            PageDown();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Called when a change in scrolling state is detected, such as a change in scroll
    /// position, extent, or viewport size.
    /// </summary>
    /// <param name="e">The event args.</param>
    /// <remarks>
    /// If you override this method, call `base.OnScrollChanged(ScrollChangedEventArgs)` to
    /// ensure that this event is raised.
    /// </remarks>
    private void OnScrollChanged(ScrollChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        RepeatButton? scrollDecreaseButton = e.NameScope.Find<RepeatButton>("PART_ScrollDecreaseButton");
        RepeatButton? scrollIncreaseButton = e.NameScope.Find<RepeatButton>("PART_ScrollIncreaseButton");

        if (scrollDecreaseButton is not null && scrollIncreaseButton is not null)
        {
            scrollDecreaseButton.Click += ScrollDecreaseButtonOnClick;
            scrollIncreaseButton.Click += ScrollIncreaseButtonOnClick;
        }

        _scrollBarExpandSubscription?.Dispose();

        _scrollBarExpandSubscription = SubscribeToScrollBars(e);
    }
    private void ScrollIncreaseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Offset += new Vector(190, 0);
    }
    private void ScrollDecreaseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Offset -= new Vector(190, 0);
    }

    private IDisposable? SubscribeToScrollBars(TemplateAppliedEventArgs e)
    {
        static IObservable<bool>? GetExpandedObservable(ScrollBar? scrollBar)
        {
            return scrollBar?.GetObservable(ScrollBar.IsExpandedProperty);
        }

        ScrollBar? horizontalScrollBar = e.NameScope.Find<ScrollBar>("PART_HorizontalScrollBar");
        ScrollBar? verticalScrollBar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollBar");

        IObservable<bool>? horizontalExpanded = GetExpandedObservable(horizontalScrollBar);
        IObservable<bool>? verticalExpanded = GetExpandedObservable(verticalScrollBar);

        IObservable<bool>? actualExpanded = null;

        if (horizontalExpanded != null && verticalExpanded != null)
        {
            actualExpanded = horizontalExpanded.CombineLatest(verticalExpanded, (h, v) => h || v);
        }
        else
        {
            if (horizontalExpanded != null)
            {
                actualExpanded = horizontalExpanded;
            }
            else if (verticalExpanded != null)
            {
                actualExpanded = verticalExpanded;
            }
        }

        return actualExpanded?.Subscribe(OnScrollBarExpandedChanged);
    }

    private void OnScrollBarExpandedChanged(bool isExpanded)
    {
        IsExpanded = isExpanded;
    }

    private void OnLayoutUpdated(object? sender, System.EventArgs e) => RaiseScrollChanged();

    private void RaiseScrollChanged()
    {
        Vector extentDelta = new(Extent.Width - _oldExtent.Width, Extent.Height - _oldExtent.Height);
        Vector offsetDelta = Offset - _oldOffset;
        Vector viewportDelta = new(Viewport.Width - _oldViewport.Width, Viewport.Height - _oldViewport.Height);

        if (!extentDelta.NearlyEquals(default) || !offsetDelta.NearlyEquals(default) ||
            !viewportDelta.NearlyEquals(default))
        {
            ScrollChangedEventArgs e = new(extentDelta, offsetDelta, viewportDelta);
            OnScrollChanged(e);

            _oldExtent = Extent;
            _oldOffset = Offset;
            _oldViewport = Viewport;
        }
    }
}