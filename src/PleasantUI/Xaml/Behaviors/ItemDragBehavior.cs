using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Transformation;
using PleasantUI.Extensions;
using PleasantUI.Xaml.Interactivity;

namespace PleasantUI.Xaml.Behaviors;

public class ItemDragBehavior : Behavior<Control>
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ItemDragBehavior, Orientation>(nameof(Orientation));

    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<double> HorizontalDragThresholdProperty =
        AvaloniaProperty.Register<ItemDragBehavior, double>(nameof(HorizontalDragThreshold), 3);

    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<double> VerticalDragThresholdProperty =
        AvaloniaProperty.Register<ItemDragBehavior, double>(nameof(VerticalDragThreshold), 3);

    private bool _enableDrag;
    private bool _dragStarted;
    private Point _start;
    private int _draggedIndex;
    private int _targetIndex;
    private ItemsControl? _itemsControl;
    private Control? _draggedContainer;

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public double HorizontalDragThreshold
    {
        get => GetValue(HorizontalDragThresholdProperty);
        set => SetValue(HorizontalDragThresholdProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public double VerticalDragThreshold
    {
        get => GetValue(VerticalDragThresholdProperty);
        set => SetValue(VerticalDragThresholdProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is null) return;

        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Bubble);
        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Bubble);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Bubble);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Bubble);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is null) return;

        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject?.Parent is not ItemsControl /*|
            AssociatedObject?.Parent is PleasantTabView { ReorderableTabs: false } |
            AssociatedObject is PleasantTabItem { CanBeDragged: false }*/) return;

        PointerPointProperties properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed && AssociatedObject?.Parent is ItemsControl itemsControl)
        {
            _enableDrag = true;
            _dragStarted = false;
            _start = e.GetPosition(AssociatedObject.Parent as Visual);
            _draggedIndex = -1;
            _targetIndex = -1;
            _itemsControl = itemsControl;
            _draggedContainer = AssociatedObject;
            _draggedContainer.ZIndex = 1;

            if (_draggedContainer != null)
            {
                SetDraggingPseudoClasses(_draggedContainer, true);
            }

            AddTransforms(_itemsControl);

            e.Pointer.Capture(AssociatedObject);
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (Equals(e.Pointer.Captured, AssociatedObject))
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Released();
            }

            e.Pointer.Capture(null);
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e) => Released();

    private void Released()
    {
        if (!_enableDrag) return;

        if (_draggedContainer is { })
        {
            _draggedContainer.ZIndex = 0;
            SetDraggingPseudoClasses(_draggedContainer, false);
        }

        RemoveTransforms(_itemsControl);

        if (_dragStarted)
        {
            if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
                MoveDraggedItem(_itemsControl, _draggedIndex, _targetIndex);
        }

        _draggedIndex = -1;
        _targetIndex = -1;
        _enableDrag = false;
        _dragStarted = false;
        _itemsControl = null;

        _draggedContainer = null;
    }

    private void AddTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null) return;

        int i = 0;

        foreach (object _ in itemsControl.Items)
        {
            Control? container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null) SetTranslateTransform(container, 0, 0);

            i++;
        }
    }

    private void RemoveTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null) return;

        int i = 0;

        foreach (object _ in itemsControl.Items)
        {
            Control? container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null) SetTranslateTransform(container, 0, 0);

            i++;
        }
    }

    private async void MoveDraggedItem(ItemsControl? itemsControl, int draggedIndex, int targetIndex)
    {
        if (itemsControl?.Items is not IList items) return;

        object? draggedItem = items[draggedIndex];
        items.RemoveAt(draggedIndex);
        items.Insert(targetIndex, draggedItem);

        if (itemsControl is SelectingItemsControl selectingItemsControl)
        {
            selectingItemsControl.SelectedIndex = -1;
            // Problem with tabs - after dragging and dropping the content disappears.
            // Solved by selecting another tab, or waiting for the null item to be selected.
            await Task.Delay(1);
            selectingItemsControl.SelectedIndex = targetIndex;
        }
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (Equals(e.Pointer.Captured, AssociatedObject) && properties.IsLeftButtonPressed)
        {
            if (_itemsControl?.Items is null || _draggedContainer?.RenderTransform is null || !_enableDrag)
                return;

            Orientation orientation = Orientation;
            Point position = e.GetPosition(_itemsControl);
            double delta = orientation == Orientation.Horizontal ? position.X - _start.X : position.Y - _start.Y;

            if (!_dragStarted)
            {
                Point diff = _start - position;

                if (orientation == Orientation.Horizontal)
                {
                    if (Math.Abs(diff.X) > HorizontalDragThreshold)
                        _dragStarted = true;
                    else return;
                }
                else
                {
                    if (Math.Abs(diff.Y) > VerticalDragThreshold)
                        _dragStarted = true;
                    else return;
                }
            }

            if (orientation == Orientation.Horizontal)
                SetTranslateTransform(_draggedContainer, delta, 0);
            else
                SetTranslateTransform(_draggedContainer, 0, delta);

            _draggedIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(_draggedContainer);
            _targetIndex = -1;

            Rect draggedBounds = _draggedContainer.Bounds;

            double draggedStart = orientation == Orientation.Horizontal ? draggedBounds.X : draggedBounds.Y;

            double draggedDeltaStart = orientation == Orientation.Horizontal
                ? draggedBounds.X + delta
                : draggedBounds.Y + delta;

            double draggedDeltaEnd = orientation == Orientation.Horizontal
                ? draggedBounds.X + delta + draggedBounds.Width
                : draggedBounds.Y + delta + draggedBounds.Height;

            int i = 0;

            foreach (object _ in _itemsControl.Items)
            {
                Control? targetContainer = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
                {
                    i++;
                    continue;
                }

                Rect targetBounds = targetContainer.Bounds;

                double targetStart = orientation == Orientation.Horizontal ? targetBounds.X : targetBounds.Y;

                double targetMid = orientation == Orientation.Horizontal
                    ? targetBounds.X + targetBounds.Width * 0.5
                    : targetBounds.Y + targetBounds.Height * 0.5;

                int targetIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(targetContainer);

                if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                        SetTranslateTransform(targetContainer, -draggedBounds.Width, 0);
                    else
                        SetTranslateTransform(targetContainer, 0, -draggedBounds.Height);

                    _targetIndex = _targetIndex == -1 ? targetIndex :
                        targetIndex > _targetIndex ? targetIndex : _targetIndex;
                }
                else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                        SetTranslateTransform(targetContainer, draggedBounds.Width, 0);
                    else
                        SetTranslateTransform(targetContainer, 0, draggedBounds.Height);

                    _targetIndex = _targetIndex == -1 ? targetIndex :
                        targetIndex < _targetIndex ? targetIndex : _targetIndex;
                }
                else
                {
                    if (orientation == Orientation.Horizontal)
                        SetTranslateTransform(targetContainer, 0, 0);
                    else
                        SetTranslateTransform(targetContainer, 0, 0);
                }

                i++;
            }
        }
    }

    private void SetDraggingPseudoClasses(Control control, bool isDragging)
    {
        if (isDragging)
            ((IPseudoClasses)control.Classes).Add(":dragging");
        else
            ((IPseudoClasses)control.Classes).Remove(":dragging");
    }

    private void SetTranslateTransform(Visual? control, double x, double y)
    {
        TransformOperations.Builder transformBuilder = new(1);
        transformBuilder.AppendTranslate(x, y);
        if (control != null) control.RenderTransform = transformBuilder.Build();
    }
}