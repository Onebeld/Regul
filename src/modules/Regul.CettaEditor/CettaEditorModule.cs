using Avalonia.Controls;
using Avalonia.Media;
using Regul.Base;
using Regul.ModuleSystem;
using System;
using System.Collections.Generic;

namespace Regul.CettaEditor
{
	public class CettaEditorModule : IModule
	{
		private int _indexLanguage = -1;

		public IImage Icon { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public string Name => "Cetta Editor";

		public string Creator => "Onebeld";

		public string Description => "Text file editor";

		public string Version => "0.1.0-alpha";

		public bool CorrectlyInitialized { get; set; }
		public bool ThereIsAnUpdate { get; set; }

		public void ChangeLanguage(string language)
		{
			throw new NotImplementedException();
		}

		public void Execute()
		{
			ModuleManager.Editors.Add(
				new Editor
				{
					Name = "Cetta Editor",
					CreatingAnEditor = () => new Views.CettaEditor(),
					Icon = App.GetResource<PathGeometry>("TheSims3Icon"),
					DialogFilters = new List<FileDialogFilter>
					{
						new FileDialogFilter {Name = "Text-files", Extensions = {"txt", "xml", "md", "cs", "axaml", "xaml", "csproj", "h", "cfg", "ini", "sgr", "py"}}
					}
				});
		}

		public void GoToUpdate() { }

		public void Release() { }
	}
}
