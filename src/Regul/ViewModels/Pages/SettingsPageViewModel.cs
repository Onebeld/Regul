using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Win32;
using PleasantUI;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Media;
using PleasantUI.Other;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Enums;
using Regul.Helpers;
using Regul.Logging;
using Regul.Managers;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Structures;
using Regul.Other;
using Regul.Structures;
using Regul.Views.Pages;
using Regul.Views.Windows;
using Language = Regul.Structures.Language;
#pragma warning disable CS4014

namespace Regul.ViewModels.Pages;

public class SettingsPageViewModel : ViewModelBase
{
    private readonly SynchronizationContext? _synchronizationContext = SynchronizationContext.Current;
    
    private Module? _selectedModule;

    private Theme? _selectedTheme;
    private bool _inRenameProcess;
    private string _renameText = string.Empty;

    private bool _isThemesChanged;
    private bool _isCheckUpdateModules;
    private bool _isCheckUpdateProgram;
    
    private string _moduleNameSearching = string.Empty;
    private bool _invertModuleList;
    private bool _sortByAlphabeticalModules;
    private LoadingWindow? _loadingWindow;
    
    public TextBox? RenameTextBox;
    
    public object? PreviousContent { get; }
    
    private TitleBarType PreviousTitleBarType { get; }
    
    public string DotNetInformation { get; } = $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}";

    public AvaloniaList<Theme> Themes { get; } = new();
    public AvaloniaList<KeyColor> Colors { get; } = new();
    public AvaloniaList<FontFamily> Fonts { get; } = new();
    public AvaloniaList<Module> SortedModules { get; } = new();
    
    public Module? SelectedModule
    {
        get => _selectedModule;
        set => RaiseAndSetIfChanged(ref _selectedModule, value);
    }

    public bool InRenameProcess
    {
        get => _inRenameProcess;
        set => RaiseAndSetIfChanged(ref _inRenameProcess, value);
    }
    public string RenameText
    {
        get => _renameText;
        set => RaiseAndSetIfChanged(ref _renameText, value);
    }
    
    public string ModuleNameSearching
    {
        get => _moduleNameSearching;
        set => RaiseAndSetIfChanged(ref _moduleNameSearching, value);
    }
    public bool InvertModuleList
    {
        get => _invertModuleList;
        set => RaiseAndSetIfChanged(ref _invertModuleList, value);
    }
    
    public bool IsSupportedOperatingSystem
    {
        get
        {
#if Windows
            return Win32Platform.WindowsVersion > new Version(10, 0, 10586);
#else
            return false;
#endif
        }
    }

    public bool IsWindows
    {
        get
        {
#if !Windows
            return false;
#else
            return true;
#endif
        }
    }

    public Theme? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            RaiseAndSetIfChanged(ref _selectedTheme, value);
            Colors.Clear();
            App.PleasantTheme.CustomTheme = value;
            PleasantUiSettings.Instance.CustomThemeModeName = value?.Name;

            if (value is not null)
            {
                foreach (KeyValuePair<string,uint> color in value.Colors)
                    Colors.Add(new KeyColor(color.Key, color.Value));
            }
        }
    }

    public FontFamily SelectedFont
    {
        get => FontFamily.Parse(PleasantUiSettings.Instance.FontName);
        set => PleasantUiSettings.Instance.FontName = value.Name;
    }

    public int SelectedIndexBlurType
    {
        get
        {
            return PleasantUiSettings.Instance.BlurMode switch
            {
                WindowTransparencyLevel.Blur => 1,
                _ => 0
            };
        }
        set
        {
            PleasantUiSettings.Instance.BlurMode = value switch
            {
                1 => WindowTransparencyLevel.Blur,
                _ => WindowTransparencyLevel.Mica
            };
        }
    }

    public int SelectedIndexCheckUpdateInterval
    {
        get
        {
            return ApplicationSettings.Current.CheckUpdateInterval switch
            {
                CheckUpdateInterval.EveryDay => 0,
                CheckUpdateInterval.EveryWeek => 1,
                CheckUpdateInterval.EveryMonth => 2,
                CheckUpdateInterval.EveryYear => 3,
                _ => 4
            };
        }
        set
        {
            ApplicationSettings.Current.CheckUpdateInterval = value switch
            {
                0 => CheckUpdateInterval.EveryDay,
                1 => CheckUpdateInterval.EveryWeek,
                2 => CheckUpdateInterval.EveryMonth,
                3 => CheckUpdateInterval.EveryYear,
                _ => CheckUpdateInterval.Never
            };
        }
    }

    public int SelectedIndexMode
    {
        get
        {
            return PleasantUiSettings.Instance.ThemeMode switch
            {
                PleasantThemeMode.Dark => 1,
                PleasantThemeMode.Mysterious => 2,
                PleasantThemeMode.Emerald => 3,
                PleasantThemeMode.Turquoise => 4,
                PleasantThemeMode.Custom => 5,
                _ => 0
            };
        }
        set
        {
            PleasantUiSettings.Instance.ThemeMode = value switch
            {
                1 => PleasantThemeMode.Dark,
                2 => PleasantThemeMode.Mysterious,
                3 => PleasantThemeMode.Emerald,
                4 => PleasantThemeMode.Turquoise,
                5 => PleasantThemeMode.Custom,
                _ => PleasantThemeMode.Light
            };
        }
    }

    public Language SelectedLanguage
    {
        get => App.Languages.First(l => l.Key == ApplicationSettings.Current.Language);
        set
        {
            ApplicationSettings.Current.Language = value.Key;
            
            Application.Current!.Styles[1] = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
            {
                Source = new Uri($"avares://Regul.Assets/Localization/{value.Key}.axaml")
            };

            foreach (Window modalWindow in WindowsManager.Windows)
            {
                if (modalWindow.Content is not null)
                    modalWindow.Content = Activator.CreateInstance(modalWindow.Content.GetType());
            }
            
            WindowsManager.MainWindow?.ChangePage(typeof(SettingsPage));
        }
    }

    public bool IsCheckUpdateModules
    {
        get => _isCheckUpdateModules;
        set => RaiseAndSetIfChanged(ref _isCheckUpdateModules, value);
    }

    public bool IsCheckUpdateProgram
    {
        get => _isCheckUpdateProgram;
        set => RaiseAndSetIfChanged(ref _isCheckUpdateProgram, value);
    }

    public bool HasUpdateInModules
    {
        get => ModuleManager.Modules.Any(module => module.HasUpdate);
    }

    public SettingsPageViewModel(object? previousContent, TitleBarType titleBarType)
    {
        PreviousContent = previousContent;
        PreviousTitleBarType = titleBarType;

        if (Directory.Exists(Directories.Themes))
        {
            foreach (string path in Directory.EnumerateFiles(Directories.Themes))
            {
                using FileStream fileStream = File.OpenRead(path);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);

                Theme theme = Theme.LoadFromText(Encoding.Default.GetString(buffer));
                (theme, _isThemesChanged) = App.PleasantTheme.CompareWithDefaultTheme(theme);
                
                Themes.Add(theme);
            }
        }

        App.PleasantTheme.DisableUpdateTheme = true;
        SelectedTheme = Themes.FirstOrDefault(t => t.Name == PleasantUiSettings.Instance.CustomThemeModeName);
        App.PleasantTheme.DisableUpdateTheme = false;

        foreach (string fontName in FontManager.Current.PlatformImpl.GetInstalledFontFamilyNames())
            Fonts.Add(FontFamily.Parse(fontName));

        ModuleManager.Modules.CollectionChanged += ModulesOnCollectionChanged;
        
        this.WhenAnyValue(x => x.ModuleNameSearching, x => x.InvertModuleList)
            .Subscribe(_ => OnSearchModules(ModuleManager.Modules));
    }
    internal void ModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSearchModules(ModuleManager.Modules);

    private void OnSearchModules(AvaloniaList<Module> modules)
    {
        SortedModules.Clear();

        List<Module> list = new(modules);

        list = list.FindAll(x => x.Instance.Instruments is { Count: > 0 });
        
        if (!string.IsNullOrWhiteSpace(ModuleNameSearching))
            list = list.FindAll(x =>
            {
                if (Application.Current is not null && Application.Current.TryFindResource(x.Instance.Name, out object? name) && name is string s)
                {
                    return s.ToLower().Contains(ModuleNameSearching);
                }

                return x.Instance.Name.ToLower().Contains(ModuleNameSearching);
            });
        
        if (InvertModuleList)
            SortedModules.AddRange(list.OrderByDescending(x => x.Instance.Name));
        else SortedModules.AddRange(list.OrderBy(x => x.Instance.Name));
    }

    public void Release()
    {
        if (!_isThemesChanged) return;

        if (!Directory.Exists(Directories.Themes))
            Directory.CreateDirectory(Directories.Themes);

        foreach (string path in Directory.EnumerateFiles(Directories.Themes))
            File.Delete(path);

        foreach (Theme theme in Themes)
        {
            if (!string.IsNullOrWhiteSpace(theme.Name))
            {
                using FileStream fileStream = File.Create(Path.Combine(Directories.Themes, $"{theme.Name}.style"));
                
                byte[] buffer = Encoding.Default.GetBytes(theme.SaveToText());
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }
    }

    public async void ResetSettings()
    {
        if (WindowsManager.MainWindow is null) return;
        
        string result = await MessageBox.Show(WindowsManager.MainWindow, "ResetSettingsWarning", string.Empty,
            new List<MessageBoxButton>
            {
                new()
                {
                    Result = "Yes",
                    Text = App.GetString("Yes")
                },
                new()
                {
                    Result = "No",
                    Text = App.GetString("No")
                }
            });
        
        if (result != "Yes") return;

        SelectedTheme = null;
        
        ApplicationSettings.Reset();
        PleasantUiSettings.Reset();
        
        Application.Current!.Styles[1] = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
        {
            Source = new Uri($"avares://Regul.Assets/Localization/{ApplicationSettings.Current.Language}.axaml")
        };
        
        RaisePropertyChanged(nameof(SelectedLanguage));
        RaisePropertyChanged(nameof(SelectedFont));
        RaisePropertyChanged(nameof(SelectedIndexMode));
        
        WindowsManager.MainWindow?.ShowNotification("SettingsHaveBeenReset", NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    public void BackToPreviousContent() => WindowsManager.MainWindow?.ChangePage(PreviousContent?.GetType(), PreviousTitleBarType);

    public async void ChangeAccentColor()
    {
        if (WindowsManager.MainWindow is null) return;
        
        Color? newColor = await ColorPickerWindow.SelectColor(WindowsManager.MainWindow, PleasantUiSettings.Instance.UIntAccentColor);

        if (newColor is { } color)
            PleasantUiSettings.Instance.UIntAccentColor = color.ToUint32();
    }

    public async void CopyAccentColor()
    {
        await Application.Current?.Clipboard?.SetTextAsync($"#{PleasantUiSettings.Instance.UIntAccentColor.ToString("x8").ToUpper()}")!;
        
        WindowsManager.MainWindow?.ShowNotification("ColorCopied", timeSpan: TimeSpan.FromSeconds(2));
    }

    public async void PasteAccentColor()
    {
        string data = await Application.Current?.Clipboard?.GetTextAsync()!;

        if (uint.TryParse(data, out uint uintColor))
            PleasantUiSettings.Instance.UIntAccentColor = uintColor;
        else if (Color.TryParse(data, out Color color))
            PleasantUiSettings.Instance.UIntAccentColor = color.ToUint32();
    }

    public async void CopyColor(KeyColor keyColor)
    {
        await Application.Current?.Clipboard?.SetTextAsync($"#{keyColor.Value.ToString("x8").ToUpper()}")!;
        
        WindowsManager.MainWindow?.ShowNotification("ColorCopied", timeSpan: TimeSpan.FromSeconds(2));
    }

    public async void PasteColor(KeyColor keyColor)
    {
        string data = await Application.Current?.Clipboard?.GetTextAsync()!;

        uint newColor;

        if (uint.TryParse(data, out uint uintColor))
            newColor = uintColor;
        else if (Color.TryParse(data, out Color color))
            newColor = color.ToUint32();
        else return;
        
        keyColor.Value = newColor;
        SelectedTheme!.Colors[keyColor.Key] = uintColor;
        _isThemesChanged = true;
            
        App.PleasantTheme.UpdateCustomTheme();
    }

    public async void ChangeColor(KeyColor keyColor)
    {
        Color? newColor = await ColorPickerWindow.SelectColor(WindowsManager.MainWindow, keyColor.Value);

        if (newColor is not null)
        {
            uint uintColor = ((Color)newColor).ToUint32();

            keyColor.Value = uintColor;
            SelectedTheme!.Colors[keyColor.Key] = uintColor;
            _isThemesChanged = true;
            
            App.PleasantTheme.UpdateCustomTheme();
        }
    }

    private string CheckAndGetThemeName(string name)
    {
        bool isCheckedOriginalName = false;
        int index = 0;

        while (true)
        {
            if (!isCheckedOriginalName && Themes.Any(t => t.Name == name))
            {
                index++;
                isCheckedOriginalName = true;
            }
            else if (Themes.Any(t => t.Name == $"{name} {index}"))
                index++;
            else break;
        }

        return index == 0 ? name : $"{name} {index}";
    }

    public void CreateTheme()
    {
        Theme theme = App.PleasantTheme.GetTheme(true);
        theme.Name = CheckAndGetThemeName(theme.Name);
        
        Themes.Add(theme);
        SelectedTheme = theme;
        _isThemesChanged = true;
    }

    public void DeleteTheme()
    {
        InRenameProcess = false;
        Themes.Remove(SelectedTheme!);
        _isThemesChanged = true;
    }

    public async void CopyTheme()
    {
        await Application.Current!.Clipboard!.SetTextAsync(SelectedTheme!.SaveToText());
        
        WindowsManager.MainWindow?.ShowNotification("ThemeCopied", timeSpan: TimeSpan.FromSeconds(2));
    }

    public async void PasteTheme(bool withoutName = false)
    {
        Theme theme;

        try
        {
            theme = Theme.LoadFromText(await Application.Current!.Clipboard!.GetTextAsync());
        }
        catch
        {
            return;
        }

        if (!withoutName)
        {
            SelectedTheme!.Name = CheckAndGetThemeName(theme.Name);
            PleasantUiSettings.Instance.CustomThemeModeName = SelectedTheme!.Name;
        }

        foreach (KeyValuePair<string,uint> color in theme.Colors)
        {
            if (SelectedTheme!.Colors.TryGetValue(color.Key, out _))
                SelectedTheme.Colors[color.Key] = color.Value;
        }
        
        App.PleasantTheme.UpdateCustomTheme();
        
        WindowsManager.MainWindow?.ShowNotification("ThemeAppliedFromClipboard", timeSpan: TimeSpan.FromSeconds(2));
    }

    public void ApplyRenameTheme()
    {
        if (string.IsNullOrWhiteSpace(RenameText))
        {
            CancelRenameTheme();
            return;
        }

        SelectedTheme!.Name = CheckAndGetThemeName(RenameText);
        PleasantUiSettings.Instance.CustomThemeModeName = SelectedTheme!.Name;
        InRenameProcess = false;
        _isThemesChanged = true;
    }

    public void CancelRenameTheme() => InRenameProcess = false;

    public void RenameTheme()
    {
        RenameText = SelectedTheme!.Name;
        
        RenameTextBox?.Focus();
        RenameTextBox?.SelectAll();

        InRenameProcess = true;
    }

    public void DeleteEditorRelatedExtension(EditorRelatedExtension editorRelatedExtension)
    {
        ApplicationSettings.Current.EditorRelatedExtensions.Remove(editorRelatedExtension);
    }
    
    public void OpenPatreon() => IoHelpers.OpenBrowserAsync("https://www.patreon.com/onebeld");

    public void OpenGitHub() => IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul");

    public void OpenGitHubModulesMd() => IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul/blob/main/modules.md");

    public void WriteEmail()
    {
        string mailto = "mailto:onebeld@gmail.com";
        Process.Start(new ProcessStartInfo
        {
            FileName = mailto,
            UseShellExecute = true,
        });
    }

    public async void CheckUpdate()
    {
        if (WindowsManager.MainWindow is null) return;
        
        IsCheckUpdateProgram = true;
        (CheckUpdateResult checkUpdateResult, Version? newVersion) = await App.CheckUpdate();

        if (checkUpdateResult == CheckUpdateResult.HasUpdate)
        {
            IsCheckUpdateProgram = false;
            string result = await MessageBox.Show(WindowsManager.MainWindow, $"{App.GetString("UpgradeProgramIsAvailable")}: {newVersion?.ToString()}", "GoToTheWebsiteToDownloadNewUpdate", new List<MessageBoxButton>()
            {
                new()
                {
                    Text = "Yes", 
                    Default = true, 
                    Result = "Yes", 
                    IsKeyDown = true
                },
                new()
                {
                    Text = "No", 
                    Result = "No"
                }
            });
            if (result == "Yes")
                IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul/tags");
        }
        else if (checkUpdateResult == CheckUpdateResult.NoUpdate)
            WindowsManager.MainWindow?.ShowNotification("NoUpdatesAtThisTime", NotificationType.Information, TimeSpan.FromSeconds(4));
        else
            WindowsManager.MainWindow?.ShowNotification("FailedToCheckForUpdates", NotificationType.Error, TimeSpan.FromSeconds(4));
        IsCheckUpdateProgram = false;
    }

    #region Modules

    [Obsolete("Obsolete")]
    public async void InstallModule()
    {
        OpenFileDialog dialog = new()
        {
            AllowMultiple = true,
            Filters = new List<FileDialogFilter>()
            {
                new()
                {
                    Name = $"ZIP {App.GetString("FilesS")}",
                    Extensions = { "zip" }
                },
                new()
                {
                    Name = $"DLL {App.GetString("FilesS")}",
                    Extensions = { "dll" }
                }
            }
        };

        string[]? result = await dialog.ShowAsync(WindowsManager.MainWindow!);
        
        if (result is not { Length: > 0 }) return;

        List<string> copiedFiles = new();

        foreach (string path in result)
        {
            if (Path.GetExtension(path).ToLower() == ".zip")
            {
                try
                {
                    copiedFiles.AddRange(ZipFileManager.ExtractToDirectoryWithPaths(path, RegulDirectories.Modules));
                }
                catch
                {
                    WindowsManager.MainWindow?.ShowNotification("Error", NotificationType.Error, TimeSpan.FromSeconds(4));
                    return;
                }
            }
            else if (Path.GetExtension(path).ToLower() == ".dll")
            {
                string pathInModulesFolder = Path.Combine(RegulDirectories.Modules, Path.GetFileName(path));

                try
                {
                    File.Copy(path, pathInModulesFolder);
                }
                catch
                {
                    WindowsManager.MainWindow?.ShowNotification("Error", NotificationType.Error, TimeSpan.FromSeconds(4));
                    return;
                }
                
                copiedFiles.Add(pathInModulesFolder);
            }
        }

        bool successfullLoad = App.LoadModules(copiedFiles);
        
        if (successfullLoad)
            WindowsManager.MainWindow?.ShowNotification("ModulesWereLoadedSuccessfully", NotificationType.Success, TimeSpan.FromSeconds(4));
    }

    public async void CheckUpdateModules()
    {
        IsCheckUpdateModules = true;

        await Task.Run(() =>
        {
            Parallel.ForEach(ModuleManager.Modules, async module =>
            {
                try
                {
                    Version? updateVersion = await module.Instance.GetNewVersion(out string? link, out Version? requiredRegulVersion);

                    if (updateVersion > module.Instance.Version)
                    {
                        module.RegulVersionRequiered = requiredRegulVersion;
                        module.LinkToUpdate = link;
                        module.NewVersion = updateVersion;
                        module.HasUpdate = true;
                    }
                }
                catch { }
            });
        });

        IsCheckUpdateModules = false;
        
        RaisePropertyChanged(nameof(HasUpdateInModules));

        if (ModuleManager.Modules.Any(x => x.HasUpdate))
            WindowsManager.MainWindow?.ShowNotification("UpdatesAreAvailableForModules", NotificationType.Success, TimeSpan.FromSeconds(4));
        else
            WindowsManager.MainWindow?.ShowNotification("ModulesHaveNoUpdate", timeSpan: TimeSpan.FromSeconds(4));
    }

    public async void BeginUpdatingModule(Module module)
    {
        if (WindowsManager.MainWindow is null) return;
        
        _loadingWindow = new LoadingWindow
        {
            Maximum = 100
        };
        _loadingWindow.Show(WindowsManager.MainWindow);
        
        await UpdateModule(module);
        
        _loadingWindow.Close();
        
        WindowsManager.MainWindow.ShowNotification("NeedToRestartToFinishUpdatingModule", timeSpan: TimeSpan.FromSeconds(5));
    }

    public async void BeginUpdatingModules()
    {
        if (WindowsManager.MainWindow is null) return;
        
        _loadingWindow = new LoadingWindow
        {
            Maximum = 100
        };
        _loadingWindow.Show(WindowsManager.MainWindow);

        foreach (Module module in ModuleManager.Modules)
        {
            if (module.HasUpdate && !module.ReadyUpgrade)
                await UpdateModule(module);
        }
        
        _loadingWindow.Close();
        
        WindowsManager.MainWindow.ShowNotification("NeedToRestartToFinishUpdatingModule", timeSpan: TimeSpan.FromSeconds(5));
    }

    private async Task UpdateModule(Module module)
    {
        try
        {
            await Task.Run(async () =>
            {
                if (!Directory.Exists(RegulDirectories.Cache))
                    Directory.CreateDirectory(RegulDirectories.Cache);
                
                WorkerProgressChanged(0, "PreparingM", true);

                string zipFile = Path.Combine(RegulDirectories.Cache, module.Instance.Name + ".zip");
                if (File.Exists(zipFile))
                {
                    ApplicationSettings.Current.UpdatableModules.Add(new UpdatableModule
                    {
                        Path = zipFile,
                        PathToModule = RegulDirectories.Modules
                    });
                    
                    _synchronizationContext?.Send(_ =>
                    {
                        module.ReadyUpgrade = true;
                    }, "");
                    
                    return;
                }
                
                if (module.LinkToUpdate is null || _loadingWindow is null) return;

                using (HttpClientDownloadWithProgress client = new(module.LinkToUpdate, zipFile))
                {
                    client.ProgressChanged += (size, downloaded, percentage) =>
                    {
                        WorkerProgressChanged(percentage ?? 0, $"{App.GetString("DownloadingM")}\n{downloaded / 1024}KB / {size / 1024}KB", false);
                    };

                    await client.StartDownload();
                }
                
                ApplicationSettings.Current.UpdatableModules.Add(new UpdatableModule
                {
                    Path = zipFile,
                    PathToModule = RegulDirectories.Modules
                });
                _synchronizationContext?.Send(_ =>
                {
                    module.ReadyUpgrade = true;
                }, "");
            });
        }
        catch (Exception e)
        {
            Logger.Instance.WriteLog(LogType.Error, e.ToString());
        }
    }

    private void WorkerProgressChanged(double progress, string userState, bool isIndeterminate)
    {
        if (_loadingWindow is null) return;
        
        _synchronizationContext?.Send(_ =>
        {
            _loadingWindow.IsIndeterminate = isIndeterminate;
            _loadingWindow.Value = progress;
            _loadingWindow.Text = App.GetString(userState);
        }, "");
    }

    #endregion
}