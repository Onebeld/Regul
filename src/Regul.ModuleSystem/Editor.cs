using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;

namespace Regul.ModuleSystem
{
	public class Editor
	{
		public string Name { get; set; }
		public Func<IEditor> CreatingAnEditor { get; set; }
		public PathGeometry Icon { get; set; }
		public List<FileDialogFilter> DialogFilters { get; set; }
	}
}