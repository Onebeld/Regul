using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Regul.Views.Pages;

public class HomePage : UserControl
{
    private Button? _menuButton;
    
    public HomePage()
    {
        AvaloniaXamlLoader.Load(this);

        _menuButton = this.FindControl<Button>("MenuButton");
    }
    private void MenuButtonsOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
            button.Command?.Execute(button.CommandParameter);

        _menuButton?.Flyout?.Hide();
    }
}