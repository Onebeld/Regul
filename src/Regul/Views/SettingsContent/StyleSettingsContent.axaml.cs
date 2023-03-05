using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Regul.Managers;
using Regul.ViewModels.Pages;
using Regul.Views.Pages;

namespace Regul.Views.SettingsContent;

public class StyleSettingsContent : UserControl
{
    private readonly SettingsPage? _settingsPage;
    
    public StyleSettingsContent()
    {
        AvaloniaXamlLoader.Load(this);

        _settingsPage = WindowsManager.MainWindow?.Content as SettingsPage;
        Unloaded += OnUnloaded;
    }
    private void OnUnloaded(object? sender, RoutedEventArgs e) => _settingsPage?.GetDataContext<SettingsPageViewModel>().Release();
}