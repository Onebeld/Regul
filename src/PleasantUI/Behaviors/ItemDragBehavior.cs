﻿using System.Collections;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using PleasantUI.Controls.Custom;

namespace PleasantUI.Behaviors
{
    public class ItemDragBehavior : Behavior<IControl>
    {
        private bool _enableDrag;
        private Point _start;
        private int _draggedIndex;
        private int _targetIndex;
        private ItemsControl _itemsControl;
        private IControl _draggedContainer;

        public static readonly StyledProperty<Orientation> OrientationProperty = 
            AvaloniaProperty.Register<ItemDragBehavior, Orientation>(nameof(Orientation));

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
            {
                AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
                AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
            }
        }
        
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject == null)
            {
                AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
                AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
                AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            }
        }

        private void Pressed(object sender, PointerPressedEventArgs e)
        {
            if (!(AssociatedObject?.Parent is ItemsControl itemsControl) | (AssociatedObject?.Parent is PleasantTabView aw && !aw.ReorderableTabs) | (AssociatedObject is PleasantTabItem at && !at.CanBeDragged))
            {
                return;
            }

            _enableDrag = true;
            _start = e.GetPosition(AssociatedObject.Parent);
            _draggedIndex = -1;
            _targetIndex = -1;
            _itemsControl = AssociatedObject.Parent as ItemsControl;
            _draggedContainer = AssociatedObject;

            AddTransforms(_itemsControl);
        }

        private void Released(object sender, PointerReleasedEventArgs e)
        {
            if (_enableDrag)
            {
                RemoveTransforms(_itemsControl);

                if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
                {
                    Debug.WriteLine($"MoveItem {_draggedIndex} -> {_targetIndex}");
                    MoveDraggedItem(_itemsControl, _draggedIndex, _targetIndex);
                }

                _draggedIndex = -1;
                _targetIndex = -1;
                _enableDrag = false;
                _itemsControl = null;
                _draggedContainer = null;
            }
        }

        private void AddTransforms(ItemsControl itemsControl)
        {
            if (itemsControl?.Items is null)
            {
                return;
            }

            int i = 0;

            foreach (object _ in itemsControl.Items)
            {
                IControl container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (!(container is null))
                {
                    container.RenderTransform = new TranslateTransform();
                }
  
                i++;
            }  
        }

        private void RemoveTransforms(ItemsControl itemsControl)
        {
            if (itemsControl?.Items is null)
            {
                return;
            }

            int i = 0;

            foreach (object _ in itemsControl.Items)
            {
                IControl container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (!(container is null))
                {
                    container.RenderTransform = null;
                }
  
                i++;
            }  
        }

        private void MoveDraggedItem(ItemsControl itemsControl, int draggedIndex, int targetIndex)
        {
            if (!(itemsControl?.Items is IList items))
            {
                return;
            }

            object draggedItem = items[draggedIndex];
            items.RemoveAt(draggedIndex);
            items.Insert(targetIndex, draggedItem);

            if (itemsControl is SelectingItemsControl selectingItemsControl)
            {
                selectingItemsControl.SelectedIndex = targetIndex;
            }
        }

        private void Moved(object sender, PointerEventArgs e)
        {
            if (_itemsControl?.Items is null || _draggedContainer is null || !_enableDrag)
            {
                return;
            }

            Orientation orientation = Orientation;
            Point position = e.GetPosition(_itemsControl);
            double delta = orientation == Orientation.Horizontal ? position.X - _start.X : position.Y - _start.Y;

            if (orientation == Orientation.Horizontal)
            {
                ((TranslateTransform) _draggedContainer.RenderTransform).X = delta;
            }
            else
            {
                ((TranslateTransform) _draggedContainer.RenderTransform).Y = delta;
            }

            _draggedIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(_draggedContainer);
            _targetIndex = -1;

            Rect draggedBounds = _draggedContainer.Bounds;

            double draggedStart = orientation == Orientation.Horizontal ? 
                draggedBounds.X : draggedBounds.Y;

            double draggedDeltaStart = orientation == Orientation.Horizontal ? 
                draggedBounds.X + delta : draggedBounds.Y + delta;

            double draggedDeltaEnd = orientation == Orientation.Horizontal ?
                draggedBounds.X + delta + draggedBounds.Width : draggedBounds.Y + delta + draggedBounds.Height;

            int i = 0;

            foreach (object _ in _itemsControl.Items)
            {
                IControl targetContainer = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
                {
                    i++;
                    continue;
                }

                Rect targetBounds = targetContainer.Bounds;

                double targetStart = orientation == Orientation.Horizontal ? 
                    targetBounds.X : targetBounds.Y;

                double targetMid = orientation == Orientation.Horizontal ? 
                    targetBounds.X + targetBounds.Width / 2 : targetBounds.Y + targetBounds.Height / 2;

                int targetIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(targetContainer);

                if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        ((TranslateTransform) targetContainer.RenderTransform).X = -draggedBounds.Width;
                    }
                    else
                    {
                        ((TranslateTransform) targetContainer.RenderTransform).Y = -draggedBounds.Height;
                    }

                    _targetIndex = _targetIndex == -1 ? 
                        targetIndex : 
                        targetIndex > _targetIndex ? targetIndex : _targetIndex;
                    Debug.WriteLine($"Moved Right {_draggedIndex} -> {_targetIndex}");
                }
                else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        ((TranslateTransform) targetContainer.RenderTransform).X = draggedBounds.Width;
                    }
                    else
                    {
                        ((TranslateTransform) targetContainer.RenderTransform).Y = draggedBounds.Height;
                    }

                    _targetIndex = _targetIndex == -1 ? 
                        targetIndex : 
                        targetIndex < _targetIndex ? targetIndex : _targetIndex;
                    Debug.WriteLine($"Moved Left {_draggedIndex} -> {_targetIndex}");
                }
                else
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        ((TranslateTransform) targetContainer.RenderTransform).X = 0;
                    }
                    else
                    {
                        ((TranslateTransform) targetContainer.RenderTransform).Y = 0;
                    }
                }

                i++;
            }

            Debug.WriteLine($"Moved {_draggedIndex} -> {_targetIndex}");
        }
    }
}