using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using PleasantUI;
using PleasantUI.Enums;
using PleasantUI.Media;
using PleasantUI.Other;
using Regul.CrashReport.ViewModels;
using Regul.CrashReport.Views;
using Regul.Enums;
using Regul.Helpers;
using Regul.Logging;
using Regul.Managers;
using Regul.ModuleSystem;
using Regul.Other;
using Regul.Structures;
using Regul.Views;
using Language = Regul.Structures.Language;
using Module = Regul.ModuleSystem.Structures.Module;

namespace Regul;

public class App : Application
{
    public static Styles ModulesLanguage { get; } = new();

    public static PleasantTheme PleasantTheme { get; private set; } = null!;

    public App() => Name = "Regul";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        DataTemplates.Add(new ViewLocator());
#endif

        Current?.Styles.Add(ModulesLanguage);

        PleasantTheme = (Current?.Styles[0] as PleasantTheme)!;

        InitializeTheme();
        InitializeLanguage();

#if DEBUG
        if (Design.IsDesignMode) return;
#endif

        if (!Directory.Exists(RegulDirectories.Modules))
            Directory.CreateDirectory(RegulDirectories.Modules);

        if (!Directory.Exists(RegulDirectories.Cache))
            Directory.CreateDirectory(RegulDirectories.Cache);

        UpdateModules();

        LoadModules(Directory.EnumerateFiles(RegulDirectories.Modules, "*.dll", SearchOption.AllDirectories));
    }

    public static void UpdateModules()
    {
        foreach (UpdatableModule updatableModule in ApplicationSettings.Current.UpdatableModules)
        {
            if (!File.Exists(updatableModule.Path)) continue;

            try
            {
                ZipFileManager.ExtractToDirectory(updatableModule.Path, RegulDirectories.Modules);
            }
            finally
            {
                File.Delete(updatableModule.Path);
            }
        }

        ApplicationSettings.Current.UpdatableModules.Clear();
    }

    private void InitializeLanguage()
    {
        Language? language = Languages.FirstOrDefault(x =>
            x.Key == ApplicationSettings.Current.Language ||
            x.AdditionalKeys.Any(lang => lang == ApplicationSettings.Current.Language));

        string? key = language.Value.Key;

        if (string.IsNullOrWhiteSpace(key))
            key = "en";

        ApplicationSettings.Current.Language = key;

        Current!.Styles[1] = new StyleInclude(new Uri("avares://Regul/App.axaml"))
        {
            Source = new Uri($"avares://Regul.Assets/Localization/{key}.axaml")
        };
    }

    private void InitializeTheme()
    {
        if (PleasantUiSettings.Instance.ThemeMode == PleasantThemeMode.Custom)
        {
            List<Theme> themes = new();

            if (Directory.Exists(Directories.Themes))
            {
                foreach (string path in Directory.EnumerateFiles(Directories.Themes, "*.style"))
                {
                    using FileStream fileStream = File.OpenRead(path);
                    themes.Add(Theme.LoadFromJson(fileStream));
                }
            }

            Theme? theme = themes.FirstOrDefault(t => t.Name == PleasantUiSettings.Instance.CustomThemeModeName);

            if (theme is not null)
            {
                (theme, _) = PleasantTheme.CompareWithDefaultTheme(theme);
                PleasantTheme.CustomTheme = theme;
            }
        }
    }

    internal static void ChangeLanguage(string key)
    {
        ApplicationSettings.Current.Language = key;

        Current!.Styles[1] = new StyleInclude(new Uri("avares://Regul/App.axaml"))
        {
            Source = new Uri($"avares://Regul.Assets/Localization/{key}.axaml")
        };
    }

    internal static async Task<(CheckUpdateResult, Version?)> CheckUpdate()
    {
        try
        {
            string result = await HttpClientHelpers.DownloadString("https://raw.githubusercontent.com/Onebeld/Regul/main/version.txt");

            Version latest = Version.Parse(result);
            Version? current = Assembly.GetExecutingAssembly().GetName().Version;

            return latest < current ? (CheckUpdateResult.HasUpdate, latest) : (CheckUpdateResult.NoUpdate, null);
        }
        catch
        {
            return (CheckUpdateResult.Error, null);
        }
    }

    public static async void InstallModules(IReadOnlyList<IStorageItem> files)
    {
        if (WindowsManager.MainWindow is null) return;

        List<IStorageItem> filesList = new(files);

        if (ApplicationSettings.Current.ScanForVirus)
        {
            WindowsManager.MainWindow.RunLoading(100);

            for (int index = filesList.Count - 1; index >= 0; index--)
            {
                string path = filesList[index].Path.AbsolutePath;
                if (!await VirusScanner.Scan(path))
                    filesList.Remove(filesList[index]);
            }

            WindowsManager.MainWindow.CloseLoading();
            
            if (filesList.Count <= 0)
                return;
        }
        
        List<string> copiedFiles = new();
        
        foreach (IStorageItem file in filesList)
        {
            if (Path.GetExtension(file.Path.AbsolutePath).ToLower() == ".zip")
            {
                try
                {
                    copiedFiles.AddRange(ZipFileManager.ExtractToDirectoryWithPaths(file.Path.AbsolutePath, RegulDirectories.Modules));
                }
                catch
                {
                    WindowsManager.MainWindow?.ShowNotification("Error", NotificationType.Error, TimeSpan.FromSeconds(4));
                    return;
                }
            }
            else if (Path.GetExtension(file.Path.AbsolutePath).ToLower() == ".dll")
            {
                string pathInModulesFolder = Path.Combine(RegulDirectories.Modules, Path.GetFileName(file.Path.AbsolutePath));

                try
                {
                    File.Copy(file.Path.AbsolutePath, pathInModulesFolder);
                }
                catch
                {
                    WindowsManager.MainWindow?.ShowNotification("Error", NotificationType.Error, TimeSpan.FromSeconds(4));
                    return;
                }

                copiedFiles.Add(pathInModulesFolder);
            }
        }

        bool successfulLoad = LoadModules(copiedFiles);

        if (successfulLoad)
            WindowsManager.MainWindow.ShowNotification("ModulesWereLoadedSuccessfully", NotificationType.Success, TimeSpan.FromSeconds(4));
    }

    public static bool LoadModules(IEnumerable<string> paths)
    {
        bool successfulLoad = true;

        foreach (string path in paths.Where(p => Path.GetExtension(p).ToLower() == ".dll"))
        {
            Module? module = null;

            try
            {
                module = ModuleManager.InitializeModule(path);

                if (module is null) continue;

                module.ChangeLanguage(ApplicationSettings.Current.Language, ModulesLanguage);
                module.Instance.Execute();
            }
            catch (Exception exception)
            {
                Logger.Instance.WriteLog(LogType.Error, $"[{exception.TargetSite?.DeclaringType}.{exception.TargetSite?.Name}()] [Failed to initialize module] {exception}", module?.PluginLoader.LoadDefaultAssembly());
                WindowsManager.MainWindow?.ShowNotification("FailedToLoadTheModule", NotificationType.Error, TimeSpan.FromSeconds(4));

                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    public static async Task<bool> UnloadModules()
    {
        for (int i = ModuleManager.Modules.Count - 1; i >= 0; i--)
        {
            Module module = ModuleManager.Modules[i];
            
            bool b = await module.Instance.Release();
            if (!b) return false;
            module.RemoveStyleLanguage(ModulesLanguage);

            ModuleManager.Modules.Remove(module);

            module.PluginLoader.Dispose();
        }

        return true;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (!ApplicationSettings.Current.HardwareAcceleration)
            PleasantUiSettings.Instance.EnableTransparency = false;

        if (ApplicationSettings.Current.ExceptionCalled)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new CrashReportWindow
                {
                    DataContext = new CrashReportViewModel(ApplicationSettings.Current.ExceptionText)
                };
            }
        }
        else
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                WindowsManager.MainWindow = new MainWindow();
                desktop.MainWindow = WindowsManager.MainWindow;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Looks for a suitable resource in the program.
    /// </summary>
    /// <param name="key">Resource name</param>
    /// <typeparam name="T">Resource type</typeparam>
    /// <returns>Resource found, otherwise null</returns>
    public static T GetResource<T>(object key)
    {
        object? value = null;

        IResourceHost? current = Current;

        while (current != null)
        {
            if (current is { } host)
            {
                if (host.TryGetResource(key, out value))
                {
                    return (T)value!;
                }
            }

            current = ((IStyleHost)current).StylingParent as IResourceHost;
        }

        return (T)value!;
    }

    public static string GetString(string key)
    {
        if (Current!.TryFindResource(key, out object? objectText))
            return objectText as string ?? string.Empty;
        return key;
    }

    public static void AddStyle(string pathToStyle)
    {
        Current?.Styles.Add(new StyleInclude(new Uri("avares://Regul/App.axaml"))
        {
            Source = new Uri(pathToStyle)
        });
    }
    
    public static void RemoveStyle(string pathToStyle)
    {
        if (Current is null) return;

        for (int index = Current.Styles.Count - 1; index >= 0; index--)
        {
            IStyle currentStyle = Current.Styles[index];
            if (currentStyle is StyleInclude { Source: { } } styleInclude && styleInclude.Source.Equals(new Uri(pathToStyle)))
            {
                Current.Styles.Remove(currentStyle);
            }
        }
    }

    public static readonly Language[] Languages =
    {
        new("English (English)", "en"),
        new("Русский (Russian)", "ru")
    };
}