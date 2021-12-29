using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.ModuleSystem;
using Regul.S3PE.Structures;

namespace Regul.S3PE
{
	public class S3PE : UserControl, IEditor
	{
		private S3PEViewModel _viewModel;

		public string FileToPath { get; set; }

		public DataGrid DataGrid { get; set; }
		public Editor CurrentEditor { get; set; }
		public Project CurrentProject { get; set; }

		public S3PE()
		{
			AvaloniaXamlLoader.Load(this);

			DataGrid = this.FindControl<DataGrid>("PART_DataGrid");

			DataGrid.SelectionChanged += (s, e) =>
			{
				if (_viewModel == null) return;

				_viewModel.SelectedResources.Clear();

				foreach (Resource resource in DataGrid.SelectedItems)
					_viewModel.SelectedResources.Add(resource);

				_viewModel.SelectedResource = DataGrid.SelectedItem as Resource;
			};
		}

		public void Load(string path, Project project, PleasantTabItem pleasantTabItem, Editor editor)
		{
			S3PEViewModel viewModel = path != null
				? new S3PEViewModel(pleasantTabItem, editor, this, path, project)
				: new S3PEViewModel(pleasantTabItem, editor, this);

			_viewModel = viewModel;

			DataContext = _viewModel;

			_viewModel.S3Pe = this;
		}

		public bool Save(string path) => _viewModel.SavePackage(path);

		public void Release() => _viewModel.Release();
	}
}