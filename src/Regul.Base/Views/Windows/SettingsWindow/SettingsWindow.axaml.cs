using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows;

public class SettingsWindow : PleasantDialogWindow
{
    public SettingsWindow()
    {
        AvaloniaXamlLoader.Load(this);

        Closed += (_, _) => this.GetDataContext<SettingsViewModel>().Release();
        TemplateApplied += (_, _) => this.GetDataContext<SettingsViewModel>().Initialize();
    }
}