using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Onebeld.Extensions;
using Regul.Base;
using Regul.Base.Other;
using Regul.ModuleSystem;

namespace Regul.SaveCleaner
{
	public class RegulSaveCleanerModule : IModule
	{
		private int _indexLanguage = -1;

		public IImage Icon { get; set; } = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri("avares://Regul.Assets/icon.ico")));
		public string Name { get; } = "RegulSaveCleaner";
		public string Creator { get; } = "Onebeld";
		public string Description { get; } = "A tool to clean saves the game The Sims™ 3 and keep it stable.";
		public string Version { get; } = "2.1.0";
		public bool CorrectlyInitialized { get; set; }
		public bool ThereIsAnUpdate { get; set; }
		public void Execute()
		{
			SaveCleanerSettings.Settings = SaveCleanerSettings.Load();

			Application.Current.Styles.Add(new StyleInclude(new Uri("resm:Style?assembly=Regul"))
			{
				Source = new Uri("avares://Regul.SaveCleaner/Icons.axaml")
			});

			Base.Views.Windows.MainViewModel viewModel =
				WindowsManager.MainWindow.GetDataContext<Base.Views.Windows.MainViewModel>();

			((AvaloniaList<IAvaloniaObject>)((MenuItem)viewModel.MenuItems[1]).Items).Add(new MenuItem
			{
				Command = Command.Create(ShowSaveCleaner)
			}.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("TheSims3SaveCleaner")));
		}

		private void ShowSaveCleaner()
		{
			MainModalWindow foundWindow = WindowsManager.FindModalWindow<MainModalWindow>();

			if (foundWindow != null && foundWindow.CanOpen)
				return;

			MainModalWindow window = new MainModalWindow();
			WindowsManager.OtherModalWindows.Add(window);
			window.Show(WindowsManager.MainWindow);
		}

		public void Release()
		{
			SaveCleanerSettings.Save();
		}

		public void ChangeLanguage(string language)
		{
			IStyle lang;

			switch (GeneralSettings.Settings.Language)
			{
				case "ru":
					lang = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
					{
						Source = new Uri($"avares://Regul.SaveCleaner/Localization/{language}.axaml")
					};
					break;
				default:
					lang = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
					{
						Source = new Uri("avares://Regul.SaveCleaner/Localization/en.axaml")
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