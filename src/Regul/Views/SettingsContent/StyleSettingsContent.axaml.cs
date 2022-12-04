using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Regul.Managers;
using Regul.ViewModels.Pages;
using Regul.Views.Pages;

// ReSharper disable UnusedParameter.Local

namespace Regul.Views.SettingsContent;

public class StyleSettingsContent : UserControl
{
    public StyleSettingsContent() => AvaloniaXamlLoader.Load(this);

    private void OnDetachedFromLogicalTree(object sender, LogicalTreeAttachmentEventArgs e)
    {
        SettingsPage? settingsPage = WindowsManager.MainWindow?.Content as SettingsPage;
        settingsPage?.GetDataContext<SettingsPageViewModel>().Release();
    }
}