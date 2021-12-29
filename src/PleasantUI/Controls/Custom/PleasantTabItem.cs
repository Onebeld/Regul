using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;
using Avalonia.Media;

namespace PleasantUI.Controls.Custom
{
    /// <summary>
    ///A Closable & Draggable TabItem
    /// </summary>
    [PseudoClasses(":dragging", ":lockdrag")]
    public class PleasantTabItem : TabItem
    {
        private Button CloseButton;

        public PleasantTabItem()
        {
            //EnableDragDrop();
        }

        static PleasantTabItem()
        {
            CanBeDraggedProperty.Changed.AddClassHandler<PleasantTabItem>((x, e) => x.OnCanDraggablePropertyChanged(x, e));
            IsSelectedProperty.Changed.AddClassHandler<PleasantTabItem>((x, e) => x.UpdatePseudoClass(x, e));
            IsClosableProperty.Changed.Subscribe(e =>
            {
                if (e.Sender is PleasantTabItem a && a.CloseButton != null) a.CloseButton.IsVisible = a.IsClosable;
            });
        }
        
        /// <summary>
        /// Is called before <see cref="PleasantTabItem.Closing"/> occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClosing(object sender, RoutedEventArgs e)
        {
            IsClosing = true;
        }

        public event EventHandler<RoutedEventArgs> Closing
        {
            add => AddHandler(ClosingEvent, value);
            remove => RemoveHandler(ClosingEvent, value);
        }

        public static readonly RoutedEvent<RoutedEventArgs> ClosingEvent =
            RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> CloseButtonClick
        {
            add => AddHandler(CloseButtonClickEvent, value);
            remove => RemoveHandler(CloseButtonClickEvent, value);
        }

        public static readonly RoutedEvent<RoutedEventArgs> CloseButtonClickEvent =
            RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(CloseButtonClick), RoutingStrategies.Bubble);
        
        private void UpdatePseudoClass(PleasantTabItem item, AvaloniaPropertyChangedEventArgs e)
        {
            if (!item.IsSelected)
            {
                item.PseudoClasses.Remove(":dragging");
            }
        }

        public void CloseCore()
        {
            TabControl x = Parent as TabControl;
            x.CloseTab(this);
        }

        /// <summary>
        /// Close the Tab
        /// </summary>
        public void Close()
        {
            RaiseEvent(new RoutedEventArgs(ClosingEvent));
            if (IsClosing)
                CloseCore();
        }

        protected void OnCanDraggablePropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            switch (CanBeDragged)
            {
                case true:
                    PseudoClasses.Add(":lockdrag");
                    break;
                case false:
                    PseudoClasses.Remove(":lockdrag");
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            CloseButton = this.GetControl<Button>(e, "PART_CloseButton");
            if (IsClosable)
            {
                CloseButton.Click += CloseButton_Click;
            }
            else
            {
                CloseButton.IsVisible = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
            Close();
        }
        
        /// <summary>
        /// Icon of the PleasantTabItem
        /// </summary>
        public Geometry Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Icon"/> property.
        /// </summary>
        public static readonly StyledProperty<Geometry> IconProperty =
            AvaloniaProperty.Register<PleasantTabItem, Geometry>(nameof(Icon));
        
        /// <summary>
        /// This property sets if the PleasantTabItem can be closed
        /// </summary>
        public bool IsClosable
        {
            get => GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="IsClosable"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsClosableProperty =
            AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsClosable), true);

        private bool _isclosing;

        /// <summary>
        /// Returns if the tab is closing.
        /// </summary>
        public bool IsClosing
        {
            get => _isclosing;
            set => SetAndRaise(IsClosingProperty, ref _isclosing, value);
        }

        /// <summary>
        /// Defines the <see cref="IsClosing"/> property.
        /// </summary>
        public static readonly DirectProperty<PleasantTabItem, bool> IsClosingProperty =
            AvaloniaProperty.RegisterDirect<PleasantTabItem, bool>(nameof(IsClosing), o => o.IsClosing);

        public bool CanBeDragged
        {
            get => GetValue(CanBeDraggedProperty);
            set => SetValue(CanBeDraggedProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="CanBeDragged"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> CanBeDraggedProperty =
            AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(CanBeDragged), true);

        public bool IsEditedIndicator
        {
            get => GetValue(IsEditedIndicatorProperty);
            set => SetValue(IsEditedIndicatorProperty, value);
        }

        public static readonly StyledProperty<bool> IsEditedIndicatorProperty =
            AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsEditedIndicator));
    }
}