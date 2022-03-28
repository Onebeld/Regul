using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace PleasantUI.Controls.Custom;

[Flags]
public enum WindowButtons
{
    Close = 0,
    CloseAndCollapse = 1,
    CloseAndExpand = 2,
    All = 3
}

public class PleasantWindow : Window, IStyleable
{
    public static readonly StyledProperty<WindowButtons> WindowButtonsProperty =
        AvaloniaProperty.Register<PleasantWindow, WindowButtons>(nameof(WindowButtons), WindowButtons.All);

    public static readonly StyledProperty<bool> FullScreenButtonProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(FullScreenButton));

    public static readonly StyledProperty<IControl> TitleBarMenuProperty =
        AvaloniaProperty.Register<PleasantWindow, IControl>(nameof(TitleBarMenu));

    public static readonly StyledProperty<bool> CancelCloseProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(CancelClose));

    public static readonly StyledProperty<Geometry> LogoProperty =
        AvaloniaProperty.Register<PleasantWindow, Geometry>(nameof(Logo));

    public static readonly StyledProperty<bool> CompactModeProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(CompactMode));

    public static readonly StyledProperty<bool> ShowPinButtonProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(ShowPinButton));

    public static readonly StyledProperty<bool> ShowCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(ShowCustomTitleBar), true);

    public static readonly StyledProperty<bool> ShowMenuBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(ShowMenuBar), true);

    private Panel? _modalWindows;

    /// <summary>
    ///     Shows or hides the Expand and Collapse buttons
    /// </summary>
    public WindowButtons WindowButtons
    {
        get => GetValue(WindowButtonsProperty);
        set => SetValue(WindowButtonsProperty, value);
    }

    /// <summary>
    ///     Shows full screen button
    /// </summary>
    public bool FullScreenButton
    {
        get => GetValue(FullScreenButtonProperty);
        set => SetValue(FullScreenButtonProperty, value);
    }

    public IControl TitleBarMenu
    {
        get => GetValue(TitleBarMenuProperty);
        set => SetValue(TitleBarMenuProperty, value);
    }

    /// <summary>
    ///     Required if the logo is drawn in vector graphics
    /// </summary>
    public Geometry Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    public bool CompactMode
    {
        get => GetValue(CompactModeProperty);
        set => SetValue(CompactModeProperty, value);
    }

    public bool CancelClose
    {
        get => GetValue(CancelCloseProperty);
        set => SetValue(CancelCloseProperty, value);
    }

    public bool ShowPinButton
    {
        get => GetValue(ShowPinButtonProperty);
        set => SetValue(ShowPinButtonProperty, value);
    }

    public bool ShowCustomTitleBar
    {
        get => GetValue(ShowCustomTitleBarProperty);
        set => SetValue(ShowCustomTitleBarProperty, value);
    }

    public bool ShowMenuBar
    {
        get => GetValue(ShowMenuBarProperty);
        set => SetValue(ShowMenuBarProperty, value);
    }

    Type IStyleable.StyleKey => typeof(PleasantWindow);

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (e.Cancel == false)
            e.Cancel = CancelClose;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        SizeToContent content = SizeToContent;

        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome |
                                      ExtendClientAreaChromeHints.OSXThickTitleBar;
        ExtendClientAreaTitleBarHeightHint = -1;

        SizeToContent = content;

        _modalWindows = e.NameScope.Get<Panel>("PART_ModalWindow");

        this.GetObservable(ShowCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
        this.GetObservable(TitleBarMenuProperty).Subscribe(val => { ShowMenuBar = val is null == false; });
    }

    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        _modalWindows?.Children.Add(modalWindow);
    }

    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        _modalWindows?.Children.Remove(modalWindow);
    }
}