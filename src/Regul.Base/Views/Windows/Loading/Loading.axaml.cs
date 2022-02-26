#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

#endregion

namespace Regul.Base.Views.Windows
{
    public class Loading : PleasantDialogWindow
    {
        public ProgressBar ProgressBar;
        public TextBlock TextBlock;

        public Loading()
        {
            AvaloniaXamlLoader.Load(this);

            ProgressBar = this.FindControl<ProgressBar>("progressBar");
            TextBlock = this.FindControl<TextBlock>("textBlock");
        }
    }
}