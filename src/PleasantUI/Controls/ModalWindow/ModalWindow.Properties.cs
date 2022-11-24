using Avalonia;
using Avalonia.Interactivity;
using PleasantUI.Interfaces;

namespace PleasantUI.Controls;

public partial class ModalWindow
{
    public event EventHandler? Closed;
    
    private object? _dialogResult = null!;

    private IPleasantWindowModal _host = null!;

    private bool _isClosed;
    private bool _isClosing;
    
    public static readonly RoutedEvent WindowClosedEvent =
        RoutedEvent.Register<ModalWindow, RoutedEventArgs>("WindowClosed", RoutingStrategies.Direct);

    public static readonly RoutedEvent WindowOpenedEvent =
        RoutedEvent.Register<ModalWindow, RoutedEventArgs>("WindowOpened", RoutingStrategies.Direct);

    public static readonly DirectProperty<ModalWindow, bool> IsClosedProperty =
        AvaloniaProperty.RegisterDirect<ModalWindow, bool>(nameof(IsClosed), o => o.IsClosed,
            (o, v) => o.IsClosed = v);

    public static readonly DirectProperty<ModalWindow, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<ModalWindow, bool>(nameof(IsClosing), o => o.IsClosing);
    
    public bool IsClosed
    {
        get => _isClosed;
        set => SetAndRaise(IsClosedProperty, ref _isClosed, value);
    }

    public bool IsClosing
    {
        get => _isClosing;
        set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    public bool CanOpen { get; set; }
}