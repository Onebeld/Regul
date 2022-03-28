using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using PleasantUI.Controls.Custom;
using PleasantUI.Data;

namespace PleasantUI.Controls.Generators;

public class PleasantTabItemContainerGenerator : ItemContainerGenerator<PleasantTabItem>
{
    private AvaloniaProperty HeaderProperty;
    private readonly AvaloniaProperty IsClosableProperty;
    private readonly AvaloniaProperty IconProperty;

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

    private IControl CreateContainer<T>(object item) where T : class, IControl, new()
    {
        if (item is T container)
        {
            return container;
        }

        T result = new();

        if (ContentTemplateProperty != null)
            result.SetValue(ContentTemplateProperty, ItemTemplate, BindingPriority.Style);

        result.SetValue(ContentProperty, item, BindingPriority.Style);

        if (item is not IControl) result.DataContext = item;

        return result;
    }


    protected override IControl CreateContainer(object item)
    {
        PleasantTabItem container = (PleasantTabItem)item;

        PleasantTabItem tabItem = (PleasantTabItem)CreateContainer<PleasantTabItem>(item);

        tabItem.Bind(TabItem.TabStripPlacementProperty,
            new OwnerBinding<Dock>(tabItem, TabControl.TabStripPlacementProperty));

        if (tabItem.HeaderTemplate is null)
            tabItem.Bind(HeaderedContentControl.HeaderTemplateProperty,
                new OwnerBinding<IDataTemplate>(tabItem, ItemsControl.ItemTemplateProperty));

        if (tabItem.Header is null)
        {
            if (item is IHeadered headered)
            {
                tabItem.Header = headered.Header;
            }
            else
            {
                if (tabItem.DataContext is not IControl) tabItem.Header = tabItem.DataContext;
            }
        }

        if (tabItem.Content is not IControl)
            tabItem.Bind(ContentControl.ContentTemplateProperty, new OwnerBinding<IDataTemplate>(
                tabItem,
                TabControl.ContentTemplateProperty));

        if (item is IPleasantTabItemTemplate tab)
        {
            tabItem.SetValue(IconProperty, tab.Icon, BindingPriority.Style);

            tabItem.SetValue(IsClosableProperty, tab.IsClosable, BindingPriority.Style);
        }

        return tabItem;
    }

    private class OwnerBinding<T> : SingleSubscriberObservableBase<T>
    {
        private readonly TabItem _item;
        private readonly StyledProperty<T> _ownerProperty;
        private IDisposable? _ownerSubscription;
        private IDisposable? _propertySubscription;

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
                _propertySubscription = tabControl.GetObservable(_ownerProperty)
                    .Subscribe(PublishNext);
        }
    }
}