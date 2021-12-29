using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.Base;

namespace Regul.SaveCleaner
{
    public class MainModalWindow : PleasantDialogWindow
    {
        public MainModalWindow()
        {
            AvaloniaXamlLoader.Load(this);
            
            TemplateApplied += (s, e) => { this.GetDataContext<MainViewModel>().LoadingSaves(); };
        }
    }
}