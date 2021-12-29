using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Onebeld.Extensions;
using Onebeld.Logging;
using PleasantUI;
using PleasantUI.Controls.Custom;
using PleasantUI.Media;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Base.Models;
using Regul.ModuleSystem;

namespace Regul.Base.Views.Windows
{
	public class SettingsViewModel : ViewModelBase
	{
		private int _theme;
		private Language _selectedLanguage;
		private Theme _customTheme;
		private IModule _selectedModule;

		private AvaloniaList<IModule> _modules = new AvaloniaList<IModule>();
		private AvaloniaList<Theme> _customThemes = new AvaloniaList<Theme>();

		#region Properties

		private int Theme
		{
			get => _theme;
			set
			{
				RaiseAndSetIfChanged(ref _theme, value);

				if (value == -1) return;

				CustomTheme = null;

				switch (value)
				{
					case 1:
						GeneralSettings.Settings.Theme = "Dark";
						break;
					case 2:
						GeneralSettings.Settings.Theme = "Mysterious";
						break;
					case 3:
						GeneralSettings.Settings.Theme = "Turquoise";
						break;
					case 4:
						GeneralSettings.Settings.Theme = "Emerald";
						break;
					default:
						GeneralSettings.Settings.Theme = "Light";
						break;
				}

				Application.Current.Styles[1] = new StyleInclude(new Uri("resm:Style?assembly=Regul"))
				{
					Source = new Uri($"avares://PleasantUI/Assets/Themes/{GeneralSettings.Settings.Theme}.axaml")
				};
			}
		}

		private AvaloniaList<Theme> CustomThemes
		{
			get => _customThemes;
			set => RaiseAndSetIfChanged(ref _customThemes, value);
		}

		private Theme CustomTheme
		{
			get => _customTheme;
			set
			{
				RaiseAndSetIfChanged(ref _customTheme, value);

				if (value == null) return;

				Theme = -1;

				GeneralSettings.Settings.Theme = value.Name;

				Application.Current.Styles[1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(value.ToAxaml());
			}
		}

		private Language SelectedLanguage
		{
			get => _selectedLanguage;
			set
			{
				RaiseAndSetIfChanged(ref _selectedLanguage, value);

				if (GeneralSettings.Settings.Language == value.Key)
					return;

				GeneralSettings.Settings.Language = value.Key;

				Application.Current.Styles[3] = new StyleInclude(new Uri("resm:Style?assembly=Regul"))
				{
					Source = new Uri(value.Resource)
				};

				foreach (IModule module in ModuleManager.System.Modules)
					module.ChangeLanguage(GeneralSettings.Settings.Language);
			}
		}

		public IModule SelectedModule
		{
			get => _selectedModule;
			set => RaiseAndSetIfChanged(ref _selectedModule, value);
		}

		public AvaloniaList<IModule> Modules
		{
			get => _modules;
			set => RaiseAndSetIfChanged(ref _modules, value);
		}

		public List<Language> Languages => App.Languages;

		#endregion

		private void Close() => WindowsManager.SettingsWindow.Close();

		public void Initialize()
		{
			LoadThemes();

			GetModules();

			// It is not possible to change the theme by analogy with changing the language, because the theme names are translated. We don't need only the English title of the theme.
			switch (GeneralSettings.Settings.Theme)
			{
				case "Light":
					Theme = 0;
					break;
				case "Dark":
					Theme = 1;
					break;
				case "Mysterious":
					Theme = 2;
					break;
				case "Turquoise":
					Theme = 3;
					break;
				case "Emerald":
					Theme = 4;
					break;
				default:
					Theme theme = CustomThemes.FirstOrDefault(t => t.Name == GeneralSettings.Settings.Theme);
					if (theme != null)
						CustomTheme = theme;
					else Theme = 0;
					break;
			}

			SelectedLanguage = Languages.FirstOrDefault( x => x.Key == GeneralSettings.Settings.Language);

			if (SelectedLanguage == null)
				SelectedLanguage = Languages.FirstOrDefault(x => x.Key == "en");
		}

		private void GetModules()
		{
			foreach (IModule module in ModuleManager.System.Modules) Modules.Add(module);
		}

		public void Release()
		{
			if (!Directory.Exists(CorePaths.Themes))
				Directory.CreateDirectory(CorePaths.Themes);

			foreach (string path in Directory.EnumerateFiles(CorePaths.Themes)) File.Delete(path);

			for (int index = 0; index < CustomThemes.Count; index++)
			{
				Theme customTheme = CustomThemes[index];
				switch (customTheme.Name)
				{
					case "Light":
					case "Dark":
					case "Mysterious":
					case "Turquoise":
					case "Emerald":
						customTheme.Name += " New";
						break;
				}

				if (!string.IsNullOrEmpty(customTheme.Name))
				{
					using (FileStream stream = File.Create(CorePaths.Themes + $"/{customTheme.Name}.xml"))
						new XmlSerializer(typeof(Theme)).Serialize(stream, customTheme);
				}
			}

			if (CustomTheme != null)
				GeneralSettings.Settings.Theme = CustomTheme.Name;
		}

		private void LoadThemes()
		{
			if (Directory.Exists("Themes"))
			{
				foreach (string path in Directory.EnumerateFiles(CorePaths.Themes, "*.xml"))
				{
					using (FileStream fs = File.OpenRead(path))
					{
						Theme theme = (Theme)new XmlSerializer(typeof(Theme)).Deserialize(fs);

						if (string.IsNullOrEmpty(theme.ID))
							theme.ID = Guid.NewGuid().ToString();

						CustomThemes.Add(theme);
					}
				}
			}
		}

		private async void AddModules()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				AllowMultiple = true,
				Filters = new List<FileDialogFilter>
				{
					new FileDialogFilter
					{
						Name = $"DLL-{App.GetResource<string>("Files")}",
						Extensions = {"dll"}
					},
					new FileDialogFilter
					{
						Name = App.GetResource<string>("AllFiles"),
						Extensions = {"*"}
					}
				}
			};

			string[] result = await dialog.ShowAsync(WindowsManager.MainWindow);

			if (result.Length > 0)
			{
				string[] copiedFiles = new string[result.Length];

				for (int index = 0; index < result.Length; index++)
				{
					string s = result[index];

					copiedFiles[index] = CorePaths.Modules + "/" + Path.GetFileName(s);
					File.Copy(s, copiedFiles[index]);
				}

				List<IModule> modules = new List<IModule>();

				foreach (string copiedFile in copiedFiles)
				{
					try
					{
						modules.Add(ModuleManager.InitializeModule(copiedFile));
					}
					catch (Exception e)
					{
						Logger.Current.WriteLog(Log.Error, $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to load module] {e}");

						await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
							App.GetResource<string>("FailedLoadModule") + $" {copiedFile}", new List<MessageBoxButton>
							{
								new MessageBoxButton
								{
									Default = true,
									Result = "OK",
									Text = "OK",
									IsKeyDown = true
								}
							}, MessageBox.MessageBoxIcon.Error, e.ToString());
					}
				}

				foreach (IModule module in modules)
				{
					if (ModuleManager.System.Modules.Contains(module))
						continue;

					InitializeModule(module, false);
				}

				int index1 = ModuleManager.System.Modules.IndexOf(SelectedModule);

				Modules.Clear();
				GetModules();

				if (index1 != -1)
					SelectedModule = Modules.ElementAt(index1);
			}
		}

		private void InitializeModule(IModule module, bool updateList = true)
		{
			try
			{
				module.Execute();
				module.ChangeLanguage(GeneralSettings.Settings.Language);
				module.CorrectlyInitialized = true;
			}
			catch (Exception e)
			{
				Logger.Current.WriteLog(Log.Error, $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to initialize module] {e}");

				MessageBox.Show(WindowsManager.MainWindow,
					App.GetResource<string>("Error"),
					App.GetResource<string>("FailedInitModule") + $" {SelectedModule.Name}",
					new List<MessageBoxButton>
					{
						new MessageBoxButton
						{
							Default = true,
							Result = "OK",
							Text = "OK",
							IsKeyDown = true
						}
					}, MessageBox.MessageBoxIcon.Error, e.ToString());
			}

			if (updateList)
			{
				int index = ModuleManager.System.Modules.IndexOf(SelectedModule);

				Modules.Clear();
				GetModules();

				SelectedModule = Modules.ElementAt(index);
			}
		}

		private void CreateTheme()
		{
			Theme theme = new Theme
			{
				Name = App.GetResource<string>("NoName"),
				ID = Guid.NewGuid().ToString(),
			};

			// So far, this is the most convenient way to create a theme, just going through almost all the properties of a new theme
			foreach (PropertyInfo property in typeof(Theme).GetProperties())
			{
				if (property.Name == "Name" || property.Name == "ID")
					continue;

				property.SetValue(theme, App.GetResource<Color>(property.Name).ToUint32());
			}

			CustomThemes.Add(theme);
		}

		private async void ChangeColor(Button buttom)
		{
			try
			{
				IBrush brush = (await WindowColorPicker.SelectColor(WindowsManager.MainWindow, ((ISolidColorBrush)buttom.Background).ToString())).ToBursh();

				buttom.Background = brush;

				Application.Current.Styles[1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(CustomTheme.ToAxaml());
			}
			catch (Exception ex)
			{
				if (!(ex is TaskCanceledException))
					Logger.Current.WriteLog(Log.Error, $"[{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}()] [Failed to change the color of the theme] {ex}");
			}
		}

		private void DeleteTheme()
		{
			CustomThemes.Remove(CustomTheme);

			Theme = 0;
		}

		private void CopyTheme()
		{
			using (StringWriter writer = new StringWriter())
			{
				new XmlSerializer(typeof(Theme)).Serialize(writer, CustomTheme);

				Application.Current.Clipboard.SetTextAsync(writer.ToString());
			}
		}

		private async void PasteTheme()
		{
			try
			{
				using (StringReader reader = new StringReader(await Application.Current.Clipboard.GetTextAsync()))
				{

					Theme theme = (Theme)new XmlSerializer(typeof(Theme)).Deserialize(reader);

					CustomThemes[CustomThemes.IndexOf(CustomTheme)] = theme;
					CustomTheme = theme;
				}
			}
			catch (Exception ex)
			{
				Logger.Current.WriteLog(Log.Error, $"[{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}()] [I couldn't insert the theme] {ex}");
			}
		}

		private void CopyColor(Button button)
		{
			Application.Current.Clipboard.SetTextAsync(ColorHelpers.ToHexColor(((SolidColorBrush)button.Background).Color));
		}

		private async void PasteColor(Button button)
		{
			string copingColor = await Application.Current.Clipboard.GetTextAsync();

			if (ColorHelpers.IsValidHexColor(copingColor))
			{
				button.Background = new SolidColorBrush(Color.Parse(copingColor));
				Application.Current.Styles[1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(CustomTheme.ToAxaml());
			}
		}
	}
}