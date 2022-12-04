using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

public partial class PleasantMiniWindow
{
    private Panel _modalWindows = null!;

    private Button? _hiddenButton;
    private Button? _closeButton;
    private Panel? _dragWindowPanel;

    public static readonly StyledProperty<bool> EnableTransparencyProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableTransparency));
    public static readonly StyledProperty<bool> ShowPinButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowPinButton), true);
    public static readonly StyledProperty<bool> ShowHiddenButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowHiddenButton));
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableCustomTitleBar), true);

    public bool EnableTransparency
    {
        get => GetValue(EnableTransparencyProperty);
        set => SetValue(EnableTransparencyProperty, value);
    }
    public bool ShowPinButton
    {
        get => GetValue(ShowPinButtonProperty);
        set => SetValue(ShowPinButtonProperty, value);
    }
    public bool ShowHiddenButton
    {
        get => GetValue(ShowHiddenButtonProperty);
        set => SetValue(ShowHiddenButtonProperty, value);
    }
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }
}