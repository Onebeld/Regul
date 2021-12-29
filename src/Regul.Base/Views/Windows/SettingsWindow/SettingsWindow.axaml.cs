using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows
{
    public class SettingsWindow : PleasantDialogWindow
    {
        public SettingsWindow()
        {
            AvaloniaXamlLoader.Load(this);

            Closed += (s, e) => this.GetDataContext<SettingsViewModel>()?.Release();
            TemplateApplied += (s, e) => this.GetDataContext<SettingsViewModel>()?.Initialize();
        }
    }
}