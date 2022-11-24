using Avalonia;

namespace PleasantUI.Controls;

public partial class ContentDialog
{
    public static readonly StyledProperty<object> BottomPanelContentProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(BottomPanelContent));

    public object BottomPanelContent
    {
        get => GetValue(BottomPanelContentProperty);
        set => SetValue(BottomPanelContentProperty, value);
    }
}