using Avalonia.Interactivity;
using PleasantUI.Enums;

namespace PleasantUI.EventArgs;

public class TitleBarTypeChangedEventArgs : RoutedEventArgs
{
    public TitleBarType NewTitleBarType { get; }

    public TitleBarTypeChangedEventArgs(RoutedEvent routedEvent, TitleBarType newTitleBarType) : base(routedEvent)
    {
        NewTitleBarType = newTitleBarType;
    }
}