using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using Regul.Controls;
using Regul.Managers;

namespace Regul.Views.Pages;

public class EditorsPage : UserControl
{
    private readonly Button? _globalMenu;
    private readonly EditorsTabView? _editorsTabView;

    public EditorsPage()
    {
        AvaloniaXamlLoader.Load(this);

        Panel? dragPanel = this.FindControl<Panel>("DragPanel");
        _globalMenu = this.FindControl<Button>("GlobalMenu");
        _editorsTabView = this.FindControl<EditorsTabView>("TabView");

        if (WindowsManager.MainWindow is { EnableCustomTitleBar: true } && dragPanel is not null)
            dragPanel.AttachTitleBar(WindowsManager.MainWindow);

        if (WindowsManager.MainWindow is { EnableCustomTitleBar: true } && WindowsManager.MainWindow.TitleBarType != TitleBarType.MacOs)
        {
            if (_editorsTabView != null)
                _editorsTabView.MarginType = TabViewMarginType.Extended;
        }
        else
        {
            if (_editorsTabView != null)
                _editorsTabView.MarginType = TabViewMarginType.Little;
        }

        TemplateApplied += (_, _) =>
        {
            if (WindowsManager.MainWindow is null) return;

            if (WindowsManager.MainWindow.ViewModel.Workbenches.Count == 0)
                WindowsManager.MainWindow.ChangePage(typeof(HomePage));
        };
    }

    // ReSharper disable once UnusedParameter.Local
    private void MenuButtonsOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
            button.Command?.Execute(button.CommandParameter);

        _globalMenu?.Flyout?.Hide();
        _editorsTabView?.AdderButton?.Flyout?.Hide();
    }
}