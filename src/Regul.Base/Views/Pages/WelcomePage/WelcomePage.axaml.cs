using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Regul.Base.Views.Pages
{
    public class WelcomePage : UserControl
    {
        public ProgressBar ProgressBar;
        public TextBlock TextBlock;
        
        public WelcomePage()
        {
            AvaloniaXamlLoader.Load(this);
            
            ProgressBar = this.FindControl<ProgressBar>("progressBar");
            TextBlock = this.FindControl<TextBlock>("textBlock");
        }
    }
}