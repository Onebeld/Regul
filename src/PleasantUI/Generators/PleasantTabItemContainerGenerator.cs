using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using PleasantUI.Controls;
using AvaloniaProperty = Avalonia.AvaloniaProperty;

namespace PleasantUI.Generators;

public class PleasantTabItemContainerGenerator : ItemContainerGenerator<PleasantTabItem>
{
    public PleasantTabItemContainerGenerator(PleasantTabView owner,
        AvaloniaProperty contentProperty,
        AvaloniaProperty contentTemplateProperty) : base(owner, contentProperty, contentTemplateProperty)
    {
    }

    private IControl CreateContainer<T>(object item) where T : class, IControl, new()
    {
        if (item is T container)
        {
            return container;
        }
        else
        {
            T result = new();

            if (ContentTemplateProperty != null)
            {
                result.SetValue(ContentTemplateProperty, ItemTemplate, BindingPriority.Style);
            }

            result.SetValue(ContentProperty, item, BindingPriority.Style);

            if (item is not IControl)
            {
                result.DataContext = item;
            }

            return result;
        }
    }

    protected override IControl? CreateContainer(object item)
    {
        PleasantTabItem? tabItem = (PleasantTabItem)CreateContainer<PleasantTabItem>(item);

        tabItem.Bind(PleasantTabItem.TabStripPlacementProperty,
            new OwnerBinding<Dock>(tabItem, PleasantTabView.TabStripPlacementProperty));

        if (tabItem.Content is not IControl)
            tabItem.Bind(PleasantTabItem.ContentTemplateProperty, new OwnerBinding<IDataTemplate?>(
                tabItem,
                PleasantTabView.ContentTemplateProperty!));
        
        if (tabItem.HeaderTemplate is null)
            tabItem.Bind(PleasantTabItem.HeaderTemplateProperty,
                new OwnerBinding<IDataTemplate>(tabItem, PleasantTabView.ItemTemplateProperty!));

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

        return tabItem;
    }

    private class OwnerBinding<T> : SingleSubscriberObservableBase<T>
    {
        private readonly TabItem? _item;
        private readonly StyledProperty<T> _ownerProperty;
        private IDisposable? _ownerSubscription;
        private IDisposable? _propertySubscription;

        public OwnerBinding(TabItem? item, StyledProperty<T> ownerProperty)
        {
            _item = item;
            _ownerProperty = ownerProperty;
        }

        protected override void Subscribed()
        {
            _ownerSubscription = ControlLocator.Track(_item ?? throw new InvalidOperationException(), 0, typeof(TabControl)).Subscribe(OwnerChanged);
        }

        protected override void Unsubscribed()
        {
            _ownerSubscription?.Dispose();
            _ownerSubscription = null;
        }

        private void OwnerChanged(ILogical? c)
        {
            _propertySubscription?.Dispose();
            _propertySubscription = null;

            if (c is TabControl tabControl)
                _propertySubscription = tabControl.GetObservable(_ownerProperty)
                    .Subscribe(PublishNext);
        }
    }
}