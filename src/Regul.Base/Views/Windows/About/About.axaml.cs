using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using System.Diagnostics;

namespace Regul.Base.Views.Windows
{
    public class About : PleasantDialogWindow
    {
		public About()
		{
			AvaloniaXamlLoader.Load(this);

			this.FindControl<Image>("AvaloniaImage").PointerReleased += About_PointerReleased1;
		}

		private void About_PointerReleased1(object sender, Avalonia.Input.PointerReleasedEventArgs e)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = "https://avaloniaui.net/",
				UseShellExecute = true
			});
		}
	}
}