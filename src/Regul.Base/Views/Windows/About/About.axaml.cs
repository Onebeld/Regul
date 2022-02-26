#region

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

#endregion

namespace Regul.Base.Views.Windows
{
    public class About : PleasantDialogWindow
    {
        public About()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Image>("AvaloniaImage").PointerReleased += About_PointerReleased1;
        }

        private void About_PointerReleased1(object sender, PointerReleasedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://avaloniaui.net/",
                UseShellExecute = true
            });
        }
    }
}