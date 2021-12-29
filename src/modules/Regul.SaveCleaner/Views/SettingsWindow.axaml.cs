using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.SaveCleaner.Views
{
    public class SettingsWindow : PleasantDialogWindow
    {
        public SettingsWindow() => AvaloniaXamlLoader.Load(this);
    }
}