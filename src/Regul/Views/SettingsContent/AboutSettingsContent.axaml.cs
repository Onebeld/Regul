using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Regul.Views.SettingsContent;

public class AboutSettingsContent : UserControl
{
    private Button? _contactTheAuthorButton;
    
    public AboutSettingsContent()
    {
        AvaloniaXamlLoader.Load(this);

        _contactTheAuthorButton = this.FindControl<Button>("ContactTheAuthorButton");
    }

    private void MenuButtonsOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
            button.Command?.Execute(button.CommandParameter);

        _contactTheAuthorButton?.Flyout?.Hide();
    }
}