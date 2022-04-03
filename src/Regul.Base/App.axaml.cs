using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace Regul.Base;

public class App : Application
{
    public static readonly Language?[] Languages = {
        new("English (English)", "en"),
        new("Русский (Russian)", "ru")
    };

    public static List<DispatcherTimer> Timers { get; set; } = new();
    public static List<Thread> Threads { get; } = new();

    public static List<Action> ActionsWhenCompleting { get; } = new();

    public static Styles ModulesLanguage { get; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        InitializeTheme();
        InitializeLanguage();
    }

    private void InitializeTheme()
    {
        PleasantTheme pleasantTheme = Current?.Styles[0] as PleasantTheme ?? throw new InvalidOperationException();
            
        string theme = string.Empty;
        foreach (string mode in Enum.GetNames(typeof(PleasantThemeMode)))
        {
            if (mode == GeneralSettings.Instance.Theme)
            {
                theme = mode;
                break;
            }
        }

        if (string.IsNullOrEmpty(theme))
        {
            List<Theme?> themes = new();

            if (Directory.Exists("Themes"))
                foreach (string path in Directory.EnumerateFiles("Themes", "*.xml"))
                {
                    using FileStream fs = File.OpenRead(path);
                    themes.Add((Theme)new XmlSerializer(typeof(Theme)).Deserialize(fs));
                }

            Theme? customTheme = themes.FirstOrDefault(t => t.Name == GeneralSettings.Instance.Theme);

            if (customTheme is not null)
            {
                pleasantTheme.CustomMode = customTheme;
                pleasantTheme.Mode = PleasantThemeMode.Custom;
            }
            else pleasantTheme.Mode = PleasantThemeMode.Light;
        }
        else
        {
            pleasantTheme.Mode = (PleasantThemeMode)Enum.Parse(typeof(PleasantThemeMode), theme);
        }
    }

    private void InitializeLanguage()
    {
        Language? language = Languages.FirstOrDefault(x => x.Key == GeneralSettings.Instance.Language)
                             ?? Languages.First(x => x.Key == "en");

        Current!.Styles[2] = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
        {
            Source = new Uri($"avares://Regul.Assets/Localization/{language?.Key}.axaml")
        };

        Current.Styles.Add(ModulesLanguage);
    }

    public static async void CheckUpdate(bool b)
    {
        string resultCheckUpdate = string.Empty;

        try
        {
            using WebClient webClient = new();
                
            webClient.DownloadStringCompleted += (_, e) =>
            {
                if (e.Error != null)
                    return;

                resultCheckUpdate = e.Result;
            };

            await webClient.DownloadStringTaskAsync(
                new Uri("https://raw.githubusercontent.com/Onebeld/Regul/main/version.txt"));

            float latest = float.Parse(resultCheckUpdate.Replace(".", ""));
            float current = float.Parse(Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                .Replace(".", "")!);

            if (!(latest > current) && b)
                await MessageBox.Show(WindowsManager.MainWindow, GetResource<string>("Information"),
                    GetResource<string>("NoNewUpdate"), new List<MessageBoxButton>
                    {
                        new()
                        {
                            Text = GetResource<string>("OK"),
                            Default = true,
                            Result = "OK",
                            IsKeyDown = true
                        }
                    }, MessageBox.MessageBoxIcon.Information);

            if (!(latest > current)) return;

            string result = await MessageBox.Show(WindowsManager.MainWindow, GetResource<string>("Information"),
                GetResource<string>("UpdateIsAvailable"), new List<MessageBoxButton>
                {
                    new()
                    {
                        Default = true,
                        Result = "Yes",
                        Text = GetResource<string>("Yes"),
                        IsKeyDown = true
                    },
                    new()
                    {
                        Result = "No",
                        Text = GetResource<string>("No")
                    }
                }, MessageBox.MessageBoxIcon.Question);

            if (result == "Yes")
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Onebeld/Regul/releases",
                    UseShellExecute = true
                });
        }
        catch (Exception e)
        {
            if (b)
                WindowsManager.ShowError(GetResource<string>("FailedCheckUpdate"), e.ToString());
        }
    }

    /// <summary>
    ///     Looks for a suitable resource.
    /// </summary>
    /// <param name="key">Resource name</param>
    /// <typeparam name="T">Resource type</typeparam>
    /// <returns>Resource found, otherwise null</returns>
    public static T GetResource<T>(object key)
    {
        object? value = null;

        IResourceHost? current = Current;

        while (current is not null)
        {
            if (current is { } host)
                if (host.TryGetResource(key, out value))
                    return (T)value!;

            current = ((IStyledElement)current).StylingParent as IResourceHost;
        }

        return (T)value!;
    }
}