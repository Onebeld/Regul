using System;
using Avalonia.Controls.Generators;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using PleasantUI.Controls.Custom;
using PleasantUI.Data;

namespace PleasantUI.Controls.Generators
{
    public class PleasantTabItemContainerGenerator : ItemContainerGenerator<PleasantTabItem>
    {
        public PleasantTabItemContainerGenerator(PleasantTabView owner,
            AvaloniaProperty contentProperty,
            AvaloniaProperty contentTemplateProperty,
            AvaloniaProperty headerProperty,
            AvaloniaProperty iconProperty,
            AvaloniaProperty<bool> isClosableProperty) : base(owner, contentProperty, contentTemplateProperty)
        {
            HeaderProperty = headerProperty;
            IconProperty = iconProperty;
            IsClosableProperty = isClosableProperty;
        }

        private AvaloniaProperty HeaderProperty, IsClosableProperty, IconProperty;

        private IControl CreateContainer<T>(object item) where T : class, IControl, new()
        {
            if (item is T container)
            {
                return container;
            }
            else
            {
                T result = new T();

                if (ContentTemplateProperty != null)
                {
                    result.SetValue(ContentTemplateProperty, ItemTemplate, BindingPriority.Style);
                }

                result.SetValue(ContentProperty, item, BindingPriority.Style);

                if (!(item is IControl))
                {
                    result.DataContext = item;
                }

                return result;
            }
        }


        protected override IControl CreateContainer(object item)
        {
            PleasantTabItem container = (PleasantTabItem)item;

            PleasantTabItem tabItem = (PleasantTabItem)CreateContainer<PleasantTabItem>(item);

            tabItem.Bind(TabItem.TabStripPlacementProperty,
                new OwnerBinding<Dock>(tabItem, TabControl.TabStripPlacementProperty));

            if (tabItem.HeaderTemplate == null)
            {
                tabItem.Bind(PleasantTabItem.HeaderTemplateProperty,
                    new OwnerBinding<IDataTemplate>(tabItem, PleasantTabView.ItemTemplateProperty));
            }

            if (tabItem.Header == null)
            {
                if (item is IHeadered headered)
                {
                    tabItem.Header = headered.Header;
                }
                else
                {
                    if (!(tabItem.DataContext is IControl))
                    {
                        tabItem.Header = tabItem.DataContext;
                    }
                }
            }

            if (!(tabItem.Content is IControl))
            {
                tabItem.Bind(TabItem.ContentTemplateProperty, new OwnerBinding<IDataTemplate>(
                    tabItem,
                    TabControl.ContentTemplateProperty));
            }

            if (item is IPleasantTabItemTemplate tab)
            {
                if (!(tab.Icon is null))
                    tabItem.SetValue(IconProperty, tab.Icon, BindingPriority.Style);

                tabItem.SetValue(IsClosableProperty, tab.IsClosable, BindingPriority.Style);
            }

            return tabItem;
        }
        
        private class OwnerBinding<T> : SingleSubscriberObservableBase<T>
        {
            private readonly TabItem _item;
            private readonly StyledProperty<T> _ownerProperty;
            private IDisposable _ownerSubscription;
            private IDisposable _propertySubscription;

            public OwnerBinding(TabItem item, StyledProperty<T> ownerProperty)
            {
                _item = item;
                _ownerProperty = ownerProperty;
            }

            protected override void Subscribed()
            {
                _ownerSubscription = ControlLocator.Track(_item, 0, typeof(TabControl)).Subscribe(OwnerChanged);
            }

            protected override void Unsubscribed()
            {
                _ownerSubscription?.Dispose();
                _ownerSubscription = null;
            }

            private void OwnerChanged(ILogical c)
            {
                _propertySubscription?.Dispose();
                _propertySubscription = null;

                if (c is TabControl tabControl)
                {
                    _propertySubscription = tabControl.GetObservable(_ownerProperty)
                        .Subscribe(x => PublishNext(x));
                }
            }
        }
    }
}