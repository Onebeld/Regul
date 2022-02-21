using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Threading;
using PleasantUI;
using PleasantUI.Media;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.ModuleSystem.Models;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Regul.Base
{
	public class App : Application
	{
		public static List<DispatcherTimer> Timers { get; set; } = new List<DispatcherTimer>();
		public static List<Thread> Threads { get; } = new List<Thread>();

		public static List<Action> ActionsWhenCompleting { get; } = new List<Action>();

		public static Styles ModulesLanguage { get; } = new Styles();

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
			
			AppCenter.Start("99222b51-2da8-4cc5-a051-81f309bb19df", typeof(Analytics), typeof(Crashes));

			InitializeTheme();
			InitializeLanguage();
		}

		private void InitializeTheme()
		{
			//DefaultTheme defaultTheme = PleasantUIDefaults.Themes.FirstOrDefault(x => x.Name == GeneralSettings.Settings.Theme);
			DefaultTheme defaultTheme = null;
			foreach (DefaultTheme item in PleasantUIDefaults.Themes)
			{
				if (item.Name == GeneralSettings.Settings.Theme)
				{
					defaultTheme = item;
					break;
				}
			}
			//

			if (defaultTheme == null)
			{
				List<Theme> Themes = new List<Theme>();

				if (Directory.Exists("Themes"))
				{
					foreach (string path in Directory.EnumerateFiles("Themes", "*.xml"))
					{
						using (FileStream fs = File.OpenRead(path))
							Themes.Add((Theme)new XmlSerializer(typeof(Theme)).Deserialize(fs));
					}
				}

				//Theme theme = Themes.FirstOrDefault(t => t.Name == GeneralSettings.Settings.Theme);
				Theme theme = null;
				for (int i = 0; i < Themes.Count; i++)
				{
					Theme item = Themes[i];

					if (item.Name == GeneralSettings.Settings.Theme)
					{
						theme = item;
						break;
					}
				}
				//

				if (theme != null)
				{
					Current.Styles[1] = AvaloniaRuntimeXamlLoader.Parse<IStyle>(theme.ToAxaml());
				}
				else
					Current.Styles[1] = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
					{
						Source = new Uri("avares://PleasantUI/Assets/Themes/Light.axaml")
					};
			}
			else Current.Styles[1] = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
			{
				Source = new Uri($"avares://PleasantUI/Assets/Themes/{defaultTheme.Name}.axaml")
			};
		}
		private void InitializeLanguage()
		{
			//Language language = Languages.FirstOrDefault(x => x.Key == GeneralSettings.Settings.Language)
			//	?? Languages.First(x => x.Key == "en");
			Language language = null;
			foreach (Language item in Languages)
			{
				if (item.Key == GeneralSettings.Settings.Language)
				{
					language = item;
					break;
				}
			}
			if (language == null)
			{
				foreach (Language item in Languages)
				{
					if (item.Key == "en")
					{
						language = item;
						break;
					}
				}
			}
			//

			Current.Styles[3] = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
			{
				Source = new Uri($"avares://Regul.Assets/Localization/{language.Key}.axaml")
			};

			Current.Styles.Add(ModulesLanguage);
		}

		public static async void CheckUpdate(bool b)
		{
			string resultCheckUpdate = string.Empty;
			string errorResult = string.Empty;

			try
			{
				using (WebClient webClient = new WebClient())
				{
					webClient.DownloadStringCompleted += (s, e) =>
					{
						if (e.Error != null)
						{
							errorResult = e.Error.ToString();
							return;
						}
						resultCheckUpdate = e.Result;
					};

					await webClient.DownloadStringTaskAsync(
						new Uri("https://raw.githubusercontent.com/Onebeld/Regul/main/version.txt"));

					float latest = float.Parse(resultCheckUpdate.Replace(".", ""));
					float current = float.Parse(Assembly.GetExecutingAssembly().GetName().Version?.ToString()
						.Replace(".", ""));

					if (!(latest > current) && b)
					{
						await MessageBox.Show(WindowsManager.MainWindow, GetResource<string>("Information"),
							GetResource<string>("NoNewUpdate"), new List<MessageBoxButton>
							{
								new MessageBoxButton
								{
									Text = GetResource<string>("OK"),
									Default = true,
									Result = "OK",
									IsKeyDown = true
								}
							}, MessageBox.MessageBoxIcon.Information);
					}

					if (!(latest > current)) return;

					string result = await MessageBox.Show(WindowsManager.MainWindow, GetResource<string>("Information"),
						GetResource<string>("UpdateIsAvailable"), new List<MessageBoxButton>
						{
							new MessageBoxButton
							{
								Default = true,
								Result = "Yes",
								Text = GetResource<string>("Yes"),
								IsKeyDown = true
							},
							new MessageBoxButton
							{
								Result = "No",
								Text = GetResource<string>("No")
							}
						}, MessageBox.MessageBoxIcon.Question);

					if (result == "Yes")
					{
						Process.Start(new ProcessStartInfo
						{
							FileName = "https://github.com/Onebeld/Regul/releases",
							UseShellExecute = true
						});
					}
				}
			}
			catch (Exception e)
			{
				if (b)
				{
					await MessageBox.Show(WindowsManager.MainWindow, GetResource<string>("Error"),
						GetResource<string>("FailedCheckUpdate"), new List<MessageBoxButton>
						{
							new MessageBoxButton
							{
								Default = true,
								Result = "OK",
								Text = GetResource<string>("OK"),
								IsKeyDown = true
							}
						}, MessageBox.MessageBoxIcon.Error, e.ToString());
				}
			}
		}

		/// <summary>
		/// Looks for a suitable resource in the program.
		/// </summary>
		/// <param name="key">Resource name</param>
		/// <typeparam name="T">Resource type</typeparam>
		/// <returns>Resource found, otherwise null</returns>
		public static T GetResource<T>(object key)
		{
			object value = null;

			IResourceHost current = Current;

			while (current != null)
			{
				if (current is IResourceHost host)
				{
					if (host.TryGetResource(key, out value))
						return (T)value;
				}

				current = ((IStyledElement)current).StylingParent as IResourceHost;
			}

			return (T)value;
		}

		public static readonly Language[] Languages = new Language[2]
		{
			new Language("English (English)", "en"),
			new Language("Русский (Russian)", "ru"),
		};
	}
}