using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using Regul.Managers;

namespace Regul.Views.Windows;

public class UserAgreementWindow : ContentDialog
{
    public UserAgreementWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static async Task<bool> Show()
    {
        if (WindowsManager.MainWindow is null) return false;
        
        UserAgreementWindow window = new UserAgreementWindow();

        window.FindControl<Button>("YesButton").Click += (_, _) =>
        {
            window.Close(true);
        };
        window.FindControl<Button>("NoButton").Click += (_, _) =>
        {
            window.Close(false);
        };

        return await window.Show<bool>(WindowsManager.MainWindow);
    }
}