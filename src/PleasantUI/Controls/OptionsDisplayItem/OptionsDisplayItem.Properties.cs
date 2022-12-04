using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace PleasantUI.Controls;

public partial class OptionsDisplayItem
{
    private bool _isPressed;
    private bool _isExpanded;
    private Border? _layoutRoot;

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, string>(nameof(Header));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, string>(nameof(Description));

    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, Geometry>(nameof(Icon));

    public static readonly StyledProperty<bool> NavigatesProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Navigates));

    public static readonly StyledProperty<IControl> ActionButtonProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, IControl>(nameof(ActionButton));

    public static readonly StyledProperty<bool> ExpandsProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Expands));

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<OptionsDisplayItem>();

    public static readonly StyledProperty<ICommand?> NavigationCommandProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, ICommand?>(nameof(NavigationCommand));

    public static readonly DirectProperty<OptionsDisplayItem, bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<OptionsDisplayItem>(x => x.IsExpanded,
            (x, v) => x.IsExpanded = v);

    public static readonly RoutedEvent<RoutedEventArgs> NavigationRequestedEvent =
        RoutedEvent.Register<OptionsDisplayItem, RoutedEventArgs>(nameof(NavigationRequested), RoutingStrategies.Bubble);

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool Navigates
    {
        get => GetValue(NavigatesProperty);
        set => SetValue(NavigatesProperty, value);
    }

    public IControl ActionButton
    {
        get => GetValue(ActionButtonProperty);
        set => SetValue(ActionButtonProperty, value);
    }

    public bool Expands
    {
        get => GetValue(ExpandsProperty);
        set => SetValue(ExpandsProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    public ICommand? NavigationCommand
    {
        get => GetValue(NavigationCommandProperty);
        set => SetValue(NavigationCommandProperty, value);
    }

    public event EventHandler<RoutedEventArgs> NavigationRequested
    {
        add => AddHandler(NavigationRequestedEvent, value);
        remove => RemoveHandler(NavigationRequestedEvent, value);
    }
}