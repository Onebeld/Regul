using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using PleasantUI;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Media;
using PleasantUI.Other;
using PleasantUI.Reactive;
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
using Language = Regul.Structures.Language;

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
    private string _editorRelatedExtension = string.Empty;
    private string _extensionSearching = string.Empty;
    private bool _invertModuleList;
    private bool _invertEditorRelatedExtensionList;

    private readonly TextBox? _renameTextBox = null!;

    private object? PreviousContent { get; }

    private TitleBarType PreviousTitleBarType { get; }

    public string DotNetInformation { get; } = $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}";

    public AvaloniaList<Theme> Themes { get; } = new();
    public AvaloniaList<FontFamily> Fonts { get; } = new();
    public AvaloniaList<Module> SortedModules { get; } = new();
    public AvaloniaList<EditorRelatedExtension> SortedEditorRelatedExtensions { get; } = new();

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

    public string DecryptedVirusTotalApiKey
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ApplicationSettings.Current.VirusTotalApiKey))
                return AesEncryption.DecryptString(ApplicationSettings.Current.Key, ApplicationSettings.Current.VirusTotalApiKey);
            return string.Empty;
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                ApplicationSettings.Current.VirusTotalApiKey = string.Empty;
            else
                ApplicationSettings.Current.VirusTotalApiKey = AesEncryption.EncryptString(ApplicationSettings.Current.Key, value);

            if (string.IsNullOrWhiteSpace(ApplicationSettings.Current.VirusTotalApiKey) || DecryptedVirusTotalApiKey.Length < 64 && ScanForVirus)
            {
                ScanForVirus = false;
                RaisePropertyChanged(nameof(ScanForVirus));
            }
        }
    }

    public bool ScanForVirus
    {
        get => ApplicationSettings.Current.ScanForVirus;
        set
        {
            if (value && string.IsNullOrWhiteSpace(ApplicationSettings.Current.VirusTotalApiKey))
            {
                WindowsManager.MainWindow?.ShowNotification("YouNeedToEnterVirusTotalApiKey", NotificationType.Error, TimeSpan.FromSeconds(3));
                RaisePropertyChanged();
                return;
            }

            if (value && DecryptedVirusTotalApiKey.Length < 64)
            {
                WindowsManager.MainWindow?.ShowNotification("ApiKeyIsTooShort", NotificationType.Error, TimeSpan.FromSeconds(3));
                RaisePropertyChanged();
                return;
            }

            ApplicationSettings.Current.ScanForVirus = value;
        }
    }

    public string ModuleNameSearching
    {
        get => _moduleNameSearching;
        set => RaiseAndSetIfChanged(ref _moduleNameSearching, value);
    }
    public string EditorRelatedExtensionSearching
    {
        get => _editorRelatedExtension;
        set => RaiseAndSetIfChanged(ref _editorRelatedExtension, value);
    }
    public string ExtensionSearching
    {
        get => _extensionSearching;
        set => RaiseAndSetIfChanged(ref _extensionSearching, value);
    }
    public bool InvertModuleList
    {
        get => _invertModuleList;
        set => RaiseAndSetIfChanged(ref _invertModuleList, value);
    }
    public bool InvertEditorRelatedExtensionList
    {
        get => _invertEditorRelatedExtensionList;
        set => RaiseAndSetIfChanged(ref _invertEditorRelatedExtensionList, value);
    }

    public bool IsSupportedOperatingSystem
    {
        get
        {
#if Windows
            return Environment.OSVersion.Version > new Version(10, 0, 10586);
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
            
            App.PleasantTheme.CustomTheme = value;
            PleasantUiSettings.Instance.CustomThemeModeName = value?.Name;
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
                PleasantThemeMode.Light => 1,
                PleasantThemeMode.Dark => 2,
                PleasantThemeMode.Mysterious => 3,
                PleasantThemeMode.Emerald => 4,
                PleasantThemeMode.Turquoise => 5,
                PleasantThemeMode.Custom => 6,
                _ => 0
            };
        }
        set
        {
            PleasantUiSettings.Instance.ThemeMode = value switch
            {
                1 => PleasantThemeMode.Light,
                2 => PleasantThemeMode.Dark,
                3 => PleasantThemeMode.Mysterious,
                4 => PleasantThemeMode.Emerald,
                5 => PleasantThemeMode.Turquoise,
                6 => PleasantThemeMode.Custom,
                _ => PleasantThemeMode.System
            };
        }
    }

    public Language SelectedLanguage
    {
        get => App.Languages.First(l => l.Key == ApplicationSettings.Current.Language);
        set
        {
            App.ChangeLanguage(value.Key);

            foreach (Window modalWindow in WindowsManager.Windows)
            {
                if (modalWindow.Content is not null)
                    modalWindow.Content = Activator.CreateInstance(modalWindow.Content.GetType());
            }

            if (WindowsManager.MainWindow is null) return;
            
            SettingsPageViewModel viewModel = new(PreviousContent, PreviousTitleBarType);
            SettingsPage settingsPage = new(viewModel);

            WindowsManager.MainWindow.ChangePage(settingsPage, TitleBarType.Classic);
        }
    }

    public bool IsCheckUpdateModules
    {
        get => _isCheckUpdateModules;
        private set => RaiseAndSetIfChanged(ref _isCheckUpdateModules, value);
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
                Theme theme = Theme.LoadFromJson(fileStream);
                
                (theme, _isThemesChanged) = App.PleasantTheme.CompareWithDefaultTheme(theme);

                Themes.Add(theme);
            }
        }

        App.PleasantTheme.DisableUpdateTheme = true;
        SelectedTheme = Themes.FirstOrDefault(t => t.Name == PleasantUiSettings.Instance.CustomThemeModeName);
        App.PleasantTheme.DisableUpdateTheme = false;

        foreach (FontFamily font in FontManager.Current.SystemFonts)
            Fonts.Add(font);
        
        ModuleManager.Modules.CollectionChanged += ModulesOnCollectionChanged;
        ApplicationSettings.Current.EditorRelatedExtensions.CollectionChanged += EditorRelatedExtensionsOnCollectionChanged;

        this.WhenAnyValue(x => x.ModuleNameSearching, x => x.InvertModuleList)
            .Subscribe(_ => OnSearchModules(ModuleManager.Modules));
        this.WhenAnyValue(x => x.EditorRelatedExtensionSearching, x => x.ExtensionSearching)
            .Subscribe(_ => OnSearchEditorRelatedExtensions(ApplicationSettings.Current.EditorRelatedExtensions));
        this.WhenAnyValue(x => x.InvertEditorRelatedExtensionList)
            .Subscribe(_ => OnSearchEditorRelatedExtensions(ApplicationSettings.Current.EditorRelatedExtensions));
    }
    private void EditorRelatedExtensionsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSearchEditorRelatedExtensions(ApplicationSettings.Current.EditorRelatedExtensions);
    internal void ModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSearchModules(ModuleManager.Modules);

    private void OnSearchModules(IEnumerable<Module> modules)
    {
        SortedModules.Clear();

        List<Module> list = new(modules);

        if (!string.IsNullOrWhiteSpace(ModuleNameSearching))
            list = list.FindAll(x => App.GetString(x.Instance.Name).ToLower().Contains(ModuleNameSearching));

        list = new List<Module>(list.OrderBy(x => x.Instance.Name));

        if (InvertModuleList)
            list.Reverse();

        SortedModules.AddRange(list);
    }

    private void OnSearchEditorRelatedExtensions(IEnumerable<EditorRelatedExtension> extensions)
    {
        SortedEditorRelatedExtensions.Clear();

        List<EditorRelatedExtension> list = new(extensions);

        if (!string.IsNullOrWhiteSpace(EditorRelatedExtensionSearching))
            list = list.FindAll(x =>
            {
                string? nameEditor = ModuleManager.GetEditorById(x.IdEditor)?.Name;
                return nameEditor is not null && App.GetString(nameEditor).ToLower().Contains(EditorRelatedExtensionSearching);
            });
        if (!string.IsNullOrWhiteSpace(ExtensionSearching))
            list = list.FindAll(x => x.Extension.ToLower().Contains(ExtensionSearching));
        
        list = new List<EditorRelatedExtension>(list.OrderBy(x => x.Extension));

        if (InvertEditorRelatedExtensionList)
            list.Reverse();

        SortedEditorRelatedExtensions.AddRange(list);
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
                theme.SaveToJson(fileStream);
            }
        }
    }

    public async void ResetSettings()
    {
        if (WindowsManager.MainWindow is null) return;

        string result = await MessageBox.Show(WindowsManager.MainWindow, "ResetSettingsWarning", string.Empty, MessageBoxButtons.YesNo);

        if (result != "Yes") return;

        SelectedTheme = null;

        ApplicationSettings.Reset();
        PleasantUiSettings.Reset();

        App.ChangeLanguage(ApplicationSettings.Current.Language);

        RaisePropertyChanged(nameof(SelectedLanguage));
        RaisePropertyChanged(nameof(SelectedFont));
        RaisePropertyChanged(nameof(SelectedIndexMode));

        WindowsManager.MainWindow.ShowNotification("SettingsHaveBeenReset", NotificationType.Success, TimeSpan.FromSeconds(3));
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
        string? data = await Application.Current?.Clipboard?.GetTextAsync();

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
        string? data = await Application.Current?.Clipboard?.GetTextAsync();

        uint newColor;

        if (uint.TryParse(data, out uint uintColor))
            newColor = uintColor;
        else if (Color.TryParse(data, out Color color))
            newColor = color.ToUint32();
        else return;

        keyColor.Value = newColor;
        SelectedTheme!.Colors.First(x => x.Key == keyColor.Key).Value = uintColor;
        _isThemesChanged = true;

        App.PleasantTheme.UpdateCustomTheme();
    }

    public async void ChangeColor(KeyColor keyColor)
    {
        if (WindowsManager.MainWindow is null) return;

        Color? newColor = await ColorPickerWindow.SelectColor(WindowsManager.MainWindow, keyColor.Value);

        if (newColor is not null)
        {
            uint uintColor = ((Color)newColor).ToUint32();

            keyColor.Value = uintColor;
            SelectedTheme!.Colors.First(x => x.Key == keyColor.Key).Value = uintColor;
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
        await Application.Current!.Clipboard!.SetTextAsync(SelectedTheme!.SaveToJson());

        WindowsManager.MainWindow?.ShowNotification("ThemeCopied", timeSpan: TimeSpan.FromSeconds(2));
    }

    public async void PasteTheme(bool withoutName = false)
    {
        Theme theme;

        try
        {
            theme = Theme.LoadFromJson(await Application.Current!.Clipboard!.GetTextAsync());
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

        foreach (KeyColor color in theme.Colors)
        {
            KeyColor? keyColor = theme.Colors.FirstOrDefault(x => x.Key == color.Key);

            if (keyColor is not null)
                color.Value = keyColor.Value;
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

        _renameTextBox?.Focus();
        _renameTextBox?.SelectAll();

        InRenameProcess = true;
    }

    public void DeleteEditorRelatedExtension(EditorRelatedExtension editorRelatedExtension)
    {
        ApplicationSettings.Current.EditorRelatedExtensions.Remove(editorRelatedExtension);
    }
    
    public void GetApiKey() => IoHelpers.OpenBrowserAsync("https://www.virustotal.com/gui/my-apikey");

    public void OpenPatreon() => IoHelpers.OpenBrowserAsync("https://www.patreon.com/onebeld");

    public void OpenGitHub() => IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul");

    public void OpenGitHubModulesMd() => IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul/blob/main/modules.md");

    public void WriteEmail()
    {
        const string mailto = "mailto:onebeld@gmail.com";
        Process.Start(new ProcessStartInfo
        {
            FileName = mailto,
            UseShellExecute = true,
        });
    }

    public void OpenDiscord() => IoHelpers.OpenBrowserAsync("https://discordapp.com/users/546992251562098690");

    public void OpenSocialNetwork()
    {
        string language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        if (language == "ru")
            IoHelpers.OpenBrowserAsync("https://vk.com/onebeld");
        else
        {
            if (WindowsManager.MainWindow is null) return;
            
            MessageBox.Show(WindowsManager.MainWindow, "FeatureIsNotSupported", string.Empty, MessageBoxButtons.Ok);
        }
    }

    public async void ReloadModules()
    {
        bool b = await App.UnloadModules();
        if (!b) return;

        await Task.Delay(200);

        App.LoadModules(Directory.EnumerateFiles(RegulDirectories.Modules, "*.dll", SearchOption.AllDirectories));
        
        WindowsManager.MainWindow?.ShowNotification("ModulesHaveBeenReloaded");
    }

    public async void CheckUpdate()
    {
        if (WindowsManager.MainWindow is null) return;

        IsCheckUpdateProgram = true;
        (CheckUpdateResult checkUpdateResult, Version? newVersion) = await App.CheckUpdate();

        if (checkUpdateResult == CheckUpdateResult.HasUpdate)
        {
            IsCheckUpdateProgram = false;
            string result = await MessageBox.Show(
                WindowsManager.MainWindow, 
                $"{App.GetString("UpgradeProgramIsAvailable")}: {newVersion?.ToString()}", 
                "GoToTheWebsiteToDownloadNewUpdate",
                MessageBoxButtons.YesNo);
            
            if (result == "Yes")
                IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul/releases");
        }
        else if (checkUpdateResult == CheckUpdateResult.NoUpdate)
            WindowsManager.MainWindow.ShowNotification("NoUpdatesAtThisTime", NotificationType.Information, TimeSpan.FromSeconds(4));
        else
            WindowsManager.MainWindow.ShowNotification("FailedToCheckForUpdates", NotificationType.Error, TimeSpan.FromSeconds(4));
        IsCheckUpdateProgram = false;
    }

    #region Modules
    
    public async void InstallModule()
    {
        IReadOnlyList<IStorageFile> files = await WindowsManager.MainWindow?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new []
            {
                new FilePickerFileType($"ZIP {App.GetString("FilesS")}")
                {
                    Patterns = new [] { "*.zip" }
                },
                new FilePickerFileType($"DLL {App.GetString("FilesS")}")
                {
                    Patterns = new [] { "*.dll" }
                }
            },
            AllowMultiple = true
        })!;

        App.InstallModules(files);
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

                    if (updateVersion <= module.Instance.Version)
                        return;

                    module.RegulVersionRequiered = requiredRegulVersion;
                    module.LinkToUpdate = link;
                    module.NewVersion = updateVersion;
                    module.HasUpdate = true;
                }
                catch
                {
                    // ignored
                }
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
        
        WindowsManager.MainWindow.RunLoading(100);

        await UpdateModule(module);

        WindowsManager.MainWindow.CloseLoading();
        WindowsManager.MainWindow.ShowNotification("NeedToRestartToFinishUpdatingModule", timeSpan: TimeSpan.FromSeconds(5));
    }

    public async void BeginUpdatingModules()
    {
        if (WindowsManager.MainWindow is null) return;

        WindowsManager.MainWindow.RunLoading(100);

        foreach (Module module in ModuleManager.Modules)
        {
            if (module.HasUpdate && !module.ReadyUpgrade)
                await UpdateModule(module);
        }
        
        WindowsManager.MainWindow.ChangeLoadingProgress(0, "PreparingM", true);
        
        bool b = await App.UnloadModules();
        if (!b)
        {
            WindowsManager.MainWindow.CloseLoading();
            return;
        }
        
        App.UpdateModules();
        App.LoadModules(Directory.EnumerateFiles(RegulDirectories.Modules, "*.dll", SearchOption.AllDirectories));

        WindowsManager.MainWindow.CloseLoading();

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
                
                WindowsManager.MainWindow?.ChangeLoadingProgress(0, "PreparingM", true);

                string zipFile = Path.Combine(RegulDirectories.Cache, module.Instance.Name + ".zip");
                if (File.Exists(zipFile))
                {
                    ApplicationSettings.Current.UpdatableModules.Add(new UpdatableModule
                    {
                        Path = zipFile, PathToModule = RegulDirectories.Modules
                    });

                    _synchronizationContext?.Send(_ =>
                    {
                        module.ReadyUpgrade = true;
                    }, "");

                    return;
                }

                if (module.LinkToUpdate is null) return;

                using (HttpClientDownloadWithProgress client = new(module.LinkToUpdate, zipFile))
                {
                    client.ProgressChanged += (size, downloaded, percentage) =>
                    {
                        WindowsManager.MainWindow?.ChangeLoadingProgress(percentage ?? 0, $"{App.GetString("DownloadingM")}\n{downloaded / 1024}KB / {size / 1024}KB", false);
                    };

                    await client.StartDownload();
                }

                ApplicationSettings.Current.UpdatableModules.Add(new UpdatableModule
                {
                    Path = zipFile, PathToModule = RegulDirectories.Modules
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

    #endregion
}