using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.ModuleSystem;

namespace Regul.CettaEditor.Views
{
	public partial class CettaEditor : UserControl, IEditor
	{
		public CettaEditor()
		{
			InitializeComponent();
		}

		public string FileToPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public Editor CurrentEditor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public Project CurrentProject { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public void Load(string path, Project project, PleasantTabItem pleasantTabItem, Editor editor)
		{
			throw new System.NotImplementedException();
		}

		public void Release()
		{
			throw new System.NotImplementedException();
		}

		public bool Save(string path)
		{
			throw new System.NotImplementedException();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
