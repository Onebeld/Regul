using System.Collections.Generic;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Regul.Base;
using Regul.ModuleSystem;

namespace Regul.S3PE
{
	public class S3PEModule : IModule
	{
		private int _indexLanguage = -1;

		public string Name { get; } = "S3PE";
		public string Creator { get; } = "Onebeld";
		public string Description { get; } = "Editor for The Sims 3 package-files";
		public string Version { get; } = "0.2.0-alpha";
		public bool CorrectlyInitialized { get; set; }
		public bool ThereIsAnUpdate { get; set; }
		public IImage Icon { get; set; } = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri("avares://Regul.S3PE/icon.png")));

		public void Execute()
		{
			Application.Current.Styles.Add(new StyleInclude(new Uri("resm:Style?assembly=Regul"))
			{
				Source = new Uri("avares://Regul.S3PE/S3PEStyles.axaml")
			});

			ModuleManager.Editors.Add(
				new Editor
				{
					Name = "The Sims 3 Package Editor",
					CreatingAnEditor = () => new S3PE(),
					Icon = App.GetResource<PathGeometry>("TheSims3Icon"),
					DialogFilters = new List<FileDialogFilter>
					{
						new FileDialogFilter {Name = "DBPF Packages", Extensions = {"package", "nhd"}}
					}
				});
		}

		public void Release() { }

		public void ChangeLanguage(string language)
		{
			IStyle lang;

			switch (GeneralSettings.Settings.Language)
			{
				case "ru":
					lang = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
					{
						Source = new Uri($"avares://Regul.S3PE/Localization/{language}.axaml")
					};
					break;
				default:
					lang = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
					{
						Source = new Uri("avares://Regul.S3PE/Localization/en.axaml")
					};
					break;
			}

			if (_indexLanguage == -1)
			{
				App.ModulesLanguage.Add(lang);
			}
			else
			{
				App.ModulesLanguage[_indexLanguage] = lang;
				return;
			}

			_indexLanguage = App.ModulesLanguage.IndexOf(lang);
		}

		public void GoToUpdate() { }
	}
}