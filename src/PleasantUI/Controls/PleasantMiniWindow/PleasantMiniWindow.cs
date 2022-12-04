using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using PleasantUI.Interfaces;

namespace PleasantUI.Controls;

public partial class PleasantMiniWindow : Window, IStyleable, IPleasantWindowModal
{
    Type IStyleable.StyleKey => typeof(PleasantMiniWindow);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindows = e.NameScope.Get<Panel>("PART_ModalWindow");

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _hiddenButton = e.NameScope.Find<Button>("PART_HiddenButton");
        _dragWindowPanel = e.NameScope.Find<Panel>("PART_DragWindow");

        if (_closeButton is not null)
            _closeButton.Click += (_, _) => Close();
        if (_hiddenButton is not null)
            _hiddenButton.Click += (_, _) => WindowState = WindowState.Minimized;

        ExtendClientAreaToDecorationsHint = PleasantUiSettings.Instance.EnableCustomTitleBar;

        if (_dragWindowPanel is not null)
            _dragWindowPanel.PointerPressed += OnDragWindowBorderOnPointerPressed;

        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
        this.GetObservable(CanResizeProperty).Subscribe(canResize =>
        {
            ExtendClientAreaTitleBarHeightHint = canResize ? 8 : 1;
        });
    }

    private void OnDragWindowBorderOnPointerPressed(object? _, PointerPressedEventArgs args)
    {
        PlatformImpl?.BeginMoveDrag(args);
    }

    public void AddModalWindow(ModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(new ModalBackground());
        windowPanel.Children.Add(modalWindow);

        _modalWindows.Children.Add(windowPanel);
    }

    public void RemoveModalWindow(ModalWindow modalWindow)
    {
        _modalWindows.Children.Remove(modalWindow.Parent!);
    }
}