using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows;

public class Loading : PleasantDialogWindow
{
    public readonly ProgressBar ProgressBar;
    public readonly TextBlock TextBlock;

    public Loading()
    {
        AvaloniaXamlLoader.Load(this);

        ProgressBar = this.FindControl<ProgressBar>("progressBar");
        TextBlock = this.FindControl<TextBlock>("textBlock");
    }
}