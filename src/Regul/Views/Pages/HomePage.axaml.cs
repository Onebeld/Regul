using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Regul.Views.Pages;

public class HomePage : UserControl
{
    private readonly Button? _menuButton;

    public HomePage()
    {
        AvaloniaXamlLoader.Load(this);

        _menuButton = this.FindControl<Button>("MenuButton");
    }

    // ReSharper disable once UnusedParameter.Local
    private void MenuButtonsOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
            button.Command?.Execute(button.CommandParameter);

        _menuButton?.Flyout?.Hide();
    }
}