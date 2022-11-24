using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

[PseudoClasses(":opened", ":closed", ":selected", ":compact")]
public partial class NavigationViewItemBase : TreeViewItem, IHeadered
{
    private object _content = "Content";

    static NavigationViewItemBase()
    {
        IsExpandedProperty.Changed.AddClassHandler<NavigationViewItemBase>(
            (x, _) =>
            {
                switch (x.IsExpanded)
                {
                    case true:
                    {
                        RoutedEventArgs routedEventArgs = new(OpenedEvent);
                        x.RaiseEvent(routedEventArgs);
                        break;
                    }
                    case false:
                    {
                        RoutedEventArgs routedEventArgs = new(ClosedEvent);
                        x.RaiseEvent(routedEventArgs);
                        break;
                    }
                }
            });
        OpenedEvent.AddClassHandler<NavigationViewItemBase>((x, e) => x.OnOpened(x, e));
        ClosedEvent.AddClassHandler<NavigationViewItemBase>((x, e) => x.OnClosed(x, e));
        IsSelectedProperty.Changed.AddClassHandler<NavigationViewItemBase>
        ((x, e) =>
        {
            switch (x.IsSelected)
            {
                case true:
                    x.OnSelected(x, e);
                    break;
                case false:
                    x.OnDeselected(x, e);
                    break;
            }
        });
        IsOpenProperty.Changed.Subscribe(OnIsOpenChanged);
        OpenPaneLengthProperty.Changed.Subscribe(OnPaneSizesChanged);
        CompactPaneLengthProperty.Changed.Subscribe(OnPaneSizesChanged);
    }
    
    private static void OnPaneSizesChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is NavigationViewItemBase n)
        {
            n.ExternalLength = n.OpenPaneLength - n.CompactPaneLength;
        }
    }
    
    private static void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NavigationViewItem sender)
        {
            if (sender.IsSelected && sender.Parent is NavigationViewItem { Parent: NavigationView nwp, SelectOnClose: true } nw)
            {
                nwp.SelectSingleItem(nw);
            }

            switch (sender.IsOpen)
            {
                case true:
                    sender.RaiseEvent(new RoutedEventArgs(OpenedEvent));
                    break;
                case false:
                    sender.IsExpanded = false;
                    sender.RaiseEvent(new RoutedEventArgs(ClosedEvent));
                    break;
            }
        }
    }
    
    public NavigationViewItemBase()
    {
        NavigationViewDistance = 0;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }
    
    protected virtual void OnDeselected(object sender, AvaloniaPropertyChangedEventArgs e)
    {

    }

    protected virtual void OnSelected(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (Parent is NavigationView { DisplayMode: SplitViewDisplayMode.CompactOverlay or SplitViewDisplayMode.Overlay } navigationView )
            navigationView.IsOpen = false;
    }

    protected virtual void OnOpened(object sender, RoutedEventArgs e)
    {
        UpdatePseudoClasses();
    }
    
    protected virtual void OnClosed(object sender, RoutedEventArgs e)
    {
        IsExpanded = false;
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        if (IsOpen)
        {
            PseudoClasses.Remove(":closed");
            PseudoClasses.Add(":opened");
        }
        else
        {
            PseudoClasses.Remove(":opened");
            PseudoClasses.Add(":closed");
        }
    }
    
    public event EventHandler<RoutedEventArgs> Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<NavigationViewItemBase, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Tunnel);

    public event EventHandler<RoutedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<NavigationViewItemBase, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Tunnel);
}