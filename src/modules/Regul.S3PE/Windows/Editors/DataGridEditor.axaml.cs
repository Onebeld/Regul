using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Regul.S3PE.Windows.Editors
{
	public partial class DataGridEditor : Window
	{
		public DataGridEditor()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
