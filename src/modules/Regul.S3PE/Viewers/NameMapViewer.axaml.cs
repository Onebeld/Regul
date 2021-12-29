using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Regul.S3PE.Interfaces;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Viewers
{
	public class NameMapViewer : UserControl, IViewer
	{
		public Button Button { get; set; }
		public TextBlock TextBlockCount { get; set; }

		public DataGrid DataGrid { get; set; }

		public NameMapViewer()
		{
			AvaloniaXamlLoader.Load(this);

			DataGrid = this.FindControl<DataGrid>("PART_DataGrid");
			Button = this.FindControl<Button>("PART_Edit");
			TextBlockCount = this.FindControl<TextBlock>("PART_Count");

			Button.Click += (s, e) =>
			{
				EditResource?.Invoke();
			};
		}
		public NameMapViewer(IResource resource, Action action) : this()
		{
			EditResource = action;
			Resource = resource;

			IDictionary<ulong, string> dictionary = Resource as IDictionary<ulong, string>;

			DataGrid.Items = dictionary;
			TextBlockCount.Text = dictionary.Count.ToString();
		}

		public IResource Resource { get; set; }
		public Action EditResource { get; set; }
	}
}