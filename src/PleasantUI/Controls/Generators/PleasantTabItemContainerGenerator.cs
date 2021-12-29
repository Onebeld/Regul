using Avalonia.Controls.Generators;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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


        protected override IControl CreateContainer(object item)
        {
            PleasantTabItem container = item as PleasantTabItem;
            if (!(item is null))
            {
                container.Bind(PleasantTabItem.TabStripPlacementProperty, Owner.GetObservable(PleasantTabView.TabStripPlacementProperty), BindingPriority.Style);
                return container;
            }
            else if (item is IPleasantTabItemTemplate temp)
            {
                PleasantTabItem tab = new PleasantTabItem();
                tab.SetValue(HeaderProperty, temp.Header, BindingPriority.Style);
                tab.SetValue(IconProperty, temp.Icon, BindingPriority.Style);
                tab.SetValue(ContentProperty, temp.Content);
                tab.SetValue(IsClosableProperty, temp.IsClosable);

                tab.Bind(PleasantTabView.TabStripPlacementProperty,
                    Owner.GetObservable(PleasantTabView.TabStripPlacementProperty), BindingPriority.Style);

                return tab;
            }
            else
            {
                PleasantTabItem tb = new PleasantTabItem();
                tb.Bind(PleasantTabItem.TabStripPlacementProperty,
                    Owner.GetObservable(PleasantTabView.TabStripPlacementProperty), BindingPriority.Style);
                return tb;
            }
        }
    }
}