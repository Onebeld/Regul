﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Enums;
using Regul.Helpers;
using Regul.Interfaces;
using Regul.Logging;
using Regul.Managers;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Structures;
using Regul.Other;
using Regul.Structures;
using Regul.ViewModels.Pages;
using Regul.ViewModels.Windows;
using Regul.Views.Pages;
using Regul.Views.Windows;
using Path = System.IO.Path;

#pragma warning disable CS0618

namespace Regul.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public SynchronizationContext? SynchronizationContext;

    private readonly EventWaitHandle _bytesWritten =
        new(false, EventResetMode.AutoReset, "Onebeld-Regul-MemoryMap-dG17tr7Nv3_BytesWritten");

    private object? _content;

    private string _searchName = string.Empty;
    private string _searchPath = string.Empty;
    private string _searchEditor = string.Empty;

    private bool _reverseProjectList;
    private bool _sortByAlphabetical;
    private bool _sortByDateOfChange = true;

    private Workbench? _selectedWorkbench;

    public AvaloniaList<Workbench> Workbenches { get; } = new();
    public AvaloniaList<Project> SortedProjects { get; } = new();

    public object? Content
    {
        get => _content;
        set => RaiseAndSetIfChanged(ref _content, value);
    }

    public Workbench? SelectedWorkbench
    {
        get => _selectedWorkbench;
        set => RaiseAndSetIfChanged(ref _selectedWorkbench, value);
    }

    public string SearchName
    {
        get => _searchName;
        set => RaiseAndSetIfChanged(ref _searchName, value);
    }

    public string SearchPath
    {
        get => _searchPath;
        set => RaiseAndSetIfChanged(ref _searchPath, value);
    }

    public string SearchEditor
    {
        get => _searchEditor;
        set => RaiseAndSetIfChanged(ref _searchEditor, value);
    }

    public bool ReverseProjectList
    {
        get => _reverseProjectList;
        set => RaiseAndSetIfChanged(ref _reverseProjectList, value);
    }

    public bool SortByAlphabetical
    {
        get => _sortByAlphabetical;
        set => RaiseAndSetIfChanged(ref _sortByAlphabetical, value);
    }

    public bool SortByDateOfChange
    {
        get => _sortByDateOfChange;
        set => RaiseAndSetIfChanged(ref _sortByDateOfChange, value);
    }

    public bool IsHomePage
    {
        get => Content is HomePage;
    }

    public MainWindowViewModel()
    {
        Content = new HomePage();

        this.WhenAnyValue(x => x.Content)
            .Subscribe(DoRaiseContentProperty);

        OnSearch(ApplicationSettings.Current.Projects);
        ApplicationSettings.Current.Projects.CollectionChanged += ProjectsOnCollectionChanged;

        this.WhenAnyValue(x => x.SearchName, x => x.SearchPath, x => x.SearchEditor)
            .Skip(1)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));
        this.WhenAnyValue(x => x.SortByAlphabetical, x => x.SortByDateOfChange)
            .Skip(1)
            .Where(x => x.Item1 || x.Item2)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));
        this.WhenAnyValue(x => x.ReverseProjectList)
            .Skip(1)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));

        CheckUpdate();
    }
    public void LaunchEventWaitHandler(object state)
    {
        SynchronizationContext? context = state as SynchronizationContext;

        while (true)
        {
            _bytesWritten.WaitOne();

            OpenCachedFilesForEdit(context);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void OpenCachedFilesForEdit(SynchronizationContext? context)
    {
        if (!Directory.Exists(RegulDirectories.Cache))
            Directory.CreateDirectory(RegulDirectories.Cache);
        if (!Directory.Exists(Path.Combine(RegulDirectories.Cache, "OpenFiles")))
            Directory.CreateDirectory(Path.Combine(RegulDirectories.Cache, "OpenFiles"));

        foreach (string file in Directory.EnumerateFiles(Path.Combine(RegulDirectories.Cache, "OpenFiles")))
        {
            string content = File.ReadAllText(file);

            File.Delete(file);

            context?.Send(_ =>
            {
                if (WindowsManager.MainWindow is null) return;

                WindowsManager.MainWindow.Topmost = true;
                WindowsManager.MainWindow.Topmost = false;
                WindowsManager.MainWindow.Activate();

                List<string> files = GetFilesFromArguments(content.Split('|'));

                DropFiles(files);
            }, "");
        }
    }

    public List<string> GetFilesFromArguments(string?[] args)
    {
        List<string> fileArgs = new();

        foreach (string? arg in args)
        {
            try
            {
                if (arg is null) continue;

                _ = new FileInfo(arg);
                fileArgs.Add(arg);
            }
            catch
            {
                // ignored
            }
        }

        return fileArgs;
    }

    private void ProjectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSearch(ApplicationSettings.Current.Projects);

    private async void CheckUpdate()
    {
        if (ApplicationSettings.Current.CheckUpdateInterval == CheckUpdateInterval.Never)
            return;

        if (!string.IsNullOrWhiteSpace(ApplicationSettings.Current.DateOfLastUpdateCheck))
        {
            DateTime dateTime = DateTime.Parse(ApplicationSettings.Current.DateOfLastUpdateCheck);
            TimeSpan timeSpan = ApplicationSettings.Current.CheckUpdateInterval switch
            {
                CheckUpdateInterval.EveryDay => TimeSpan.FromDays(1),
                CheckUpdateInterval.EveryWeek => TimeSpan.FromDays(7),
                CheckUpdateInterval.EveryMonth => TimeSpan.FromDays(30),
                CheckUpdateInterval.EveryYear => TimeSpan.FromDays(365),
                _ => throw new ArgumentOutOfRangeException()
            };

            dateTime = dateTime.Add(timeSpan);

            if (dateTime > DateTime.Now)
                return;
        }

        // Check Regul update
        (CheckUpdateResult checkUpdateResult, Version? newVersion) = await App.CheckUpdate();

        if (checkUpdateResult == CheckUpdateResult.HasUpdate && newVersion is not null && WindowsManager.MainWindow is not null)
        {
            string result = await MessageBox.Show(WindowsManager.MainWindow, $"{App.GetString("UpgradeProgramIsAvailable")}: {newVersion.ToString()}", "GoToTheWebsiteToDownloadNewUpdate",
                new List<MessageBoxButton>
                {
                    new()
                    {
                        Text = App.GetString("Yes"), Default = true, Result = "Yes", IsKeyDown = true
                    },
                    new()
                    {
                        Text = App.GetString("No"), Result = "No"
                    }
                });
            if (result == "Yes")
            {
#pragma warning disable CS4014
                IoHelpers.OpenBrowserAsync("https://github.com/Onebeld/Regul/tags");
#pragma warning restore CS4014
            }
        }

        // Check modules update
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
                catch
                {
                    // ignored
                }
            });
        });
        if (ModuleManager.Modules.Any(x => x.HasUpdate))
            WindowsManager.MainWindow?.ShowNotification("UpdatesAreAvailableForModules", NotificationType.Warning, TimeSpan.FromSeconds(4));

        ApplicationSettings.Current.DateOfLastUpdateCheck = DateTime.Now.ToString(CultureInfo.CurrentCulture);
    }

    private void OnSearch(AvaloniaList<Project> obj)
    {
        SortedProjects.Clear();

        if (obj.Count == 0) return;

        List<Project> list = new(obj);

        if (!string.IsNullOrWhiteSpace(SearchName))
            list = list.FindAll(x => Path.GetFileName(x.Path).ToLower().Contains(SearchName.ToLower()));
        if (!string.IsNullOrWhiteSpace(SearchPath))
            list = list.FindAll(x => x.Path.ToLower().Contains(SearchPath.ToLower()));
        if (!string.IsNullOrWhiteSpace(SearchEditor))
            list = list.FindAll(x =>
            {
                string? nameEditor = ModuleManager.GetEditorById(x.IdEditor)?.Name;
                return nameEditor is not null && App.GetString(nameEditor).ToLower().Contains(SearchEditor);
            });

        if (SortByDateOfChange)
            list = new List<Project>(list.OrderByDescending(x => DateTime.Parse(x.DateTime)));
        else if (SortByAlphabetical)
            list = new List<Project>(list.OrderBy(x => Path.GetFileNameWithoutExtension(x.Path)));

        if (ReverseProjectList)
            list.Reverse();

        SortedProjects.AddRange(list);
    }

    private void DoRaiseContentProperty(object? obj) => RaisePropertyChanged(nameof(IsHomePage));

    public void OpenSettings(TitleBarType titleBarType = TitleBarType.Classic)
    {
        SettingsPageViewModel viewModel = new(Content, titleBarType);
        SettingsPage settingsPage = new(viewModel);

        WindowsManager.MainWindow!.ChangePage(settingsPage, TitleBarType.NavigationView);
    }

    public void OpenTools()
    {
        bool hasInstruments = ModuleManager.Modules.Any(module => module.Instance.Instruments is { Count: > 0 });

        if (!hasInstruments)
        {
            WindowsManager.MainWindow?.ShowNotification("ToolsNotInstalled", NotificationType.Warning, TimeSpan.FromSeconds(5));
            return;
        }

        ToolWindow toolWindow = new()
        {
            DataContext = new ToolWindowViewModel()
        };
        toolWindow.Show(WindowsManager.MainWindow!);
    }

    public void GoToHome()
    {
        WindowsManager.MainWindow?.ChangePage(typeof(HomePage));
    }

    public void GoToEditors()
    {
        WindowsManager.MainWindow?.ChangePage(typeof(EditorsPage), TitleBarType.ExtendedWithoutContent);
    }

    public async void CreateProject()
    {
        if (Content is not HomePage && Content is not EditorsPage)
            return;

        if (ModuleManager.Editors.Count < 1)
        {
            WindowsManager.MainWindow?.ShowNotification("EditorsNotInstalled", NotificationType.Error, TimeSpan.FromSeconds(5));

            return;
        }

        (Editor? editor, _) = await EditorSelectionWindow.GetEditor(null, false);

        if (editor is not null)
        {
            Workbench workbench = CreateWorkbench(null, editor);

            SelectedWorkbench = workbench;

            if (Content is not EditorsPage)
                WindowsManager.MainWindow?.ChangePage(typeof(EditorsPage), TitleBarType.ExtendedWithoutContent);
        }
    }

    public void OpenProject(Project project)
    {
        bool isExistWorkbench = TryGetExistenceWorkbench(project.Path, out Workbench? existWorkbench);

        if (isExistWorkbench)
        {
            SelectedWorkbench = existWorkbench;

            if (Content is not EditorsPage)
                WindowsManager.MainWindow?.ChangePage(typeof(EditorsPage), TitleBarType.ExtendedWithoutContent);

            WindowsManager.MainWindow?.ShowNotification("ProjectAlreadyOpen", NotificationType.Warning, TimeSpan.FromSeconds(5));
            return;
        }

        Editor? editor = ModuleManager.GetEditorById(project.IdEditor);

        if (editor is null)
        {
            WindowsManager.MainWindow?.ShowNotification("EditorsNotInstalledForProject", NotificationType.Error, TimeSpan.FromSeconds(5));
            return;
        }

        if (!File.Exists(project.Path))
        {
            WindowsManager.MainWindow?.ShowNotification("ThisFileDoesNotExist", NotificationType.Error, TimeSpan.FromSeconds(5));
            ApplicationSettings.Current.Projects.Remove(project);
            return;
        }

        AddProject(project.Path, project.IdEditor);

        Workbench workbench = CreateWorkbench(project.Path, editor);

        SelectedWorkbench = workbench;

        if (Content is not EditorsPage)
            WindowsManager.MainWindow?.ChangePage(typeof(EditorsPage), TitleBarType.ExtendedWithoutContent);
    }

    [Obsolete("Obsolete")]
    public async void OpenFile()
    {
        if (Content is not HomePage && Content is not EditorsPage)
            return;

        if (ModuleManager.Editors.Count < 1)
        {
            WindowsManager.MainWindow?.ShowNotification("EditorsNotInstalled", NotificationType.Error, TimeSpan.FromSeconds(5));
            return;
        }

        string[]? paths = await OpenFilePicker();
        if (paths is null || paths.Length < 1)
            return;

        foreach (string s in paths)
        {
            (Editor? editor, bool openExtension) = await GetEditor(s);

            if (editor is null) continue;

            SaveExtension(openExtension, paths[0], editor.Id);
            LoadFile(s, editor);

            AddProject(s, editor.Id);
        }
    }

    private static void AddProject(string path, ulong editorId)
    {
        Project? project = ApplicationSettings.Current.Projects.FirstOrDefault(p => p.Path == path);

        if (project is null)
        {
            project = new Project
            {
                Path = path, DateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture), IdEditor = editorId
            };
            ApplicationSettings.Current.Projects.Add(project);
        }
        else project.DateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
    }

    public void LoadFile(string path, Editor editor)
    {
        if (Workbenches.Any(x => x.PathToFile == path))
        {
            WindowsManager.MainWindow?.ShowNotification($"{App.GetString("ThisFileIsAlreadyOpen")}: {Path.GetFileName(path)}", NotificationType.Information, TimeSpan.FromSeconds(4));
            return;
        }

        CreateWorkbench(path, editor);

        SelectedWorkbench = Workbenches.ElementAt(Workbenches.Count - 1);

        if (Content is not EditorsPage)
            WindowsManager.MainWindow?.ChangePage(typeof(EditorsPage), TitleBarType.ExtendedWithoutContent);
    }

    private void SaveExtension(bool openExtension, string path, ulong idEditor)
    {
        if (!openExtension) return;

        ApplicationSettings.Current.EditorRelatedExtensions.Add(new EditorRelatedExtension
        {
            Extension = Path.GetExtension(path), IdEditor = idEditor
        });
    }

    public void OpenProjectsWindow()
    {
        OpenProjectWindow openProjectWindow = new();
        openProjectWindow.Show(WindowsManager.MainWindow!);
    }

    public void OpenLogWindow()
    {
        PleasantMiniWindow window = WindowsManager.CreateMiniWindow();
        window.Content = new LogWindow();
        window.DataContext = new LogWindowViewModel(window);
        window.CanResize = false;
        window.Width = 500;
        window.Height = 450;
        window.Icon = WindowsManager.MainWindow?.Icon;

        window.Bind(Window.TitleProperty, new DynamicResourceExtension("Logs"));

        window.Show();
    }

    public void DeleteProject(Project project) => ApplicationSettings.Current.Projects.Remove(project);

    #region DragAndDrop

    public void DropModules(IEnumerable<string?> files)
    {
        List<string> copiedFiles = new();

        foreach (string? file in files)
        {
            if (file is null) continue;

            if (Path.GetExtension(file) == ".zip")
            {
                try
                {
                    copiedFiles.AddRange(ZipFileManager.ExtractToDirectoryWithPaths(file, RegulDirectories.Modules));
                }
                catch
                {
                    WindowsManager.MainWindow?.ShowNotification("FailedToLoadTheModules", NotificationType.Error, TimeSpan.FromSeconds(4));
                    return;
                }
            }
            else if (Path.GetExtension(file) == ".dll")
            {
                string path = Path.Combine(RegulDirectories.Modules, Path.GetFileName(file));
                try
                {
                    File.Copy(file, path);
                }
                catch
                {
                    WindowsManager.MainWindow?.ShowNotification("FailedToLoadTheModules", NotificationType.Error, TimeSpan.FromSeconds(4));
                    return;
                }

                copiedFiles.Add(path);
            }
        }

        App.LoadModules(copiedFiles);

        OnSearch(ApplicationSettings.Current.Projects);

        WindowsManager.MainWindow?.ShowNotification("ModulesWereLoadedSuccessfully", NotificationType.Success, TimeSpan.FromSeconds(4));
    }

    public async void DropFiles(IEnumerable<string?> files)
    {
        if (ModuleManager.Editors.Count < 1)
        {
            WindowsManager.MainWindow?.ShowNotification("EditorsNotInstalled", NotificationType.Error, TimeSpan.FromSeconds(5));
            return;
        }

        foreach (string? file in files)
        {
            if (file is null) continue;

            (Editor? editor, bool openExtension) = await GetEditor(file);

            if (editor is null) continue;

            SaveExtension(openExtension, file, editor.Id);

            LoadFile(file, editor);
        }
    }

    private async Task<(Editor?, bool)> GetEditor(string file)
    {
        Editor? editor = null;
        bool openExtension = false;

        foreach (EditorRelatedExtension editorRelatedExtension in ApplicationSettings.Current.EditorRelatedExtensions)
        {
            if (Path.GetExtension(file) == editorRelatedExtension.Extension)
            {
                editor = ModuleManager.GetEditorById(editorRelatedExtension.IdEditor);
                if (editor is not null)
                    break;
            }
        }

        if (editor is null)
            (editor, openExtension) = await EditorSelectionWindow.GetEditor(Path.GetFileName(file));

        return (editor, openExtension);
    }

    #endregion

    #region Workbench Interactions

    private bool _showNotificationAfterSaving = true;

    public Workbench CreateWorkbench(string? path, Editor editor)
    {
        Workbench workbench = new()
        {
            EditorViewModel = (IEditorViewModel?)Activator.CreateInstance(editor.Type), EditorId = editor.Id, PathToFile = path, IsDirty = path is null
        };
        if (workbench.EditorViewModel != null)
        {
            workbench.EditorViewModel.Workbench = workbench;
            workbench.EditorViewModel.Execute();
        }

        Workbenches.Add(workbench);

        return workbench;
    }

    public async Task<SaveResult> SaveWorkbench(Workbench? workbench)
    {
        if (workbench?.EditorViewModel is null) return SaveResult.Error;

        if (workbench.PathToFile is null)
        {
            Editor? editor = ModuleManager.GetEditorById(workbench.EditorId);

            if (editor is null) return SaveResult.Error;

            string? path = await OpenSaveFilePicker(editor);

            if (path is null) return SaveResult.Cancel;
            workbench.PathToFile = path;
        }

        SaveResult saveResult;

        try
        {
            saveResult = workbench.EditorViewModel.Save();
        }
        catch (Exception e)
        {
            Logger.Instance.WriteLog(LogType.Error, $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] {e}\r\n");

            if (_showNotificationAfterSaving)
            {
                WindowsManager.MainWindow?.ShowNotification(
                    $"{App.GetString("ProjectSaveError")}: {Path.GetFileName(workbench.PathToFile)}",
                    NotificationType.Error,
                    TimeSpan.FromSeconds(4));
            }

            return SaveResult.Error;
        }

        if (saveResult == SaveResult.Success)
        {
            workbench.IsDirty = false;

            Project? project = ApplicationSettings.Current.Projects.FirstOrDefault(p => p.Path == workbench.PathToFile);

            if (project is null)
            {
                project = new Project
                {
                    Path = workbench.PathToFile, DateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture), IdEditor = workbench.EditorId
                };
                ApplicationSettings.Current.Projects.Add(project);
            }
            else project.DateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);

            AddProject(workbench.PathToFile, workbench.EditorId);

            if (_showNotificationAfterSaving)
            {
                OnSearch(ApplicationSettings.Current.Projects);

                WindowsManager.MainWindow?.ShowNotification(
                    $"{App.GetString("ProjectSavedSuccessfully")}: {Path.GetFileName(workbench.PathToFile)}",
                    NotificationType.Success,
                    TimeSpan.FromSeconds(4));
            }
        }

        return SaveResult.Success;
    }

    public async void SaveAsWorkbench()
    {
        if (SelectedWorkbench is null) return;

        Editor? editor = ModuleManager.GetEditorById(SelectedWorkbench.EditorId);

        if (editor is null) return;

        string? path = await OpenSaveFilePicker(editor);

        if (path is null)
            return;

        Project? project = ApplicationSettings.Current.Projects.FirstOrDefault(p => p.Path == SelectedWorkbench.PathToFile);

        if (project is not null)
            project.Path = path;

        SelectedWorkbench.PathToFile = path;

        await SaveWorkbench(SelectedWorkbench);
    }

    public async void SaveAllWorkbenches()
    {
        _showNotificationAfterSaving = false;
        foreach (Workbench workbench in Workbenches)
        {
            await SaveWorkbench(workbench);
        }
        _showNotificationAfterSaving = true;

        OnSearch(ApplicationSettings.Current.Projects);

        WindowsManager.MainWindow?.ShowNotification("AllProjectsSavedSuccessfully", NotificationType.Success, TimeSpan.FromSeconds(4));
    }

    #endregion

    #region FilePicker

    [Obsolete("Obsolete")]
    public async Task<string[]?> OpenFilePicker()
    {
        /*IReadOnlyList<IStorageFile> files = await WindowsManager.MainWindow?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = editor.FileTypes
        })!;

        List<string> pathList = new();

        foreach (IStorageFile storageFile in files)
        {
            storageFile.TryGetUri(out Uri? uri);

            if (uri is null) continue;

            bool f = TryGetExistenceWorkbench(uri.LocalPath, out _);
            if (f) continue;

            bool isExistWorkbench = TryGetExistenceWorkbench(uri.LocalPath, out _);

            if (isExistWorkbench)
            {
                WindowsManager.MainWindow?.ShowNotification("ProjectAlreadyOpen", NotificationType.Warning, TimeSpan.FromSeconds(5));
                continue;
            }

            pathList.Add(uri.LocalPath);
        }

        return pathList;*/

        OpenFileDialog dialog = new()
        {
            Filters = new List<FileDialogFilter>(), AllowMultiple = true
        };
        dialog.Filters.Add(new FileDialogFilter
        {
            Name = App.GetString("AllFiles"),
            Extensions =
            {
                "*"
            }
        });

        return await dialog.ShowAsync(WindowsManager.MainWindow!);
    }

    [Obsolete("Obsolete")]
    public async Task<string?> OpenSaveFilePicker(Editor editor)
    {
        /*IStorageFile? file = await WindowsManager.MainWindow?.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = editor.FileTypes
        })!;
        
        if (file is null) return null;
        file.TryGetUri(out Uri? uri);

        return uri?.LocalPath;*/

        SaveFileDialog saveFileDialog = new()
        {
            Filters = new List<FileDialogFilter>()
        };
        foreach (FilePickerFileType fileType in editor.FileTypes)
        {
            saveFileDialog.Filters.Add(new FileDialogFilter
            {
                Name = fileType.Name, Extensions = fileType.Patterns!.Select(x => x.Remove(0, 2)).ToList()
            });
        }

        return await saveFileDialog.ShowAsync(WindowsManager.MainWindow!);
    }

    #endregion

    private bool TryGetExistenceWorkbench(string fileToPath, out Workbench? workbench)
    {
        workbench = Workbenches.FirstOrDefault(w => w.PathToFile == fileToPath);
        return workbench is not null;
    }

    public static void OpenFileBrowser(Workbench workbench)
    {
        if (workbench.PathToFile is null) return;

        IoHelpers.OpenFileInFileExplorer(workbench.PathToFile);

#if Windows
        Process.Start("explorer.exe", "/select, " + workbench.PathToFile);
#endif
    }

    public void ClearGc()
    {
        WindowsManager.MainWindow?.ShowNotification("MemorySuccessfullyCleared", NotificationType.Success, TimeSpan.FromSeconds(3));

        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public void Exit() => WindowsManager.MainWindow?.Close();
}