using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Onebeld.Extensions;
using Onebeld.Logging;
using PleasantUI.Controls.Custom;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Base.Generators;
using Regul.Base.Other;
using Regul.Base.Views.Pages;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Models;
using Regul.Settings;

namespace Regul.Base.Views.Windows;

public class MainViewModel : ViewModelBase
{
    private readonly EventWaitHandle _bytesWritten =
        new(false, EventResetMode.AutoReset, "Onebeld-Regul-MemoryMap-dG17tr7Nv3_BytesWritten");

    private readonly List<string?> _fileArgs = new();
    
    private IManagedNotificationManager _notificationManager;
    private object _page;
    private string _searchText;

    private int _selectedPatternSearch;
    private Project _selectedProject;
    private PleasantTabItem _selectedTab;
        
    public AvaloniaList<IRegulObject?> RegulMenuItems { get; set; }

    #region Properties

    public IManagedNotificationManager NotificationManager
    {
        get => _notificationManager;
        set => RaiseAndSetIfChanged(ref _notificationManager, value);
    }

    public object Page
    {
        get => _page;
        set => RaiseAndSetIfChanged(ref _page, value);
    }

    public AvaloniaList<IAvaloniaObject> MenuItems => MenuGenerator.Generate(RegulMenuItems);

    public AvaloniaList<PleasantTabItem> TabItems { get; } = new();

    public PleasantTabItem SelectedTab
    {
        get => _selectedTab;
        set => RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public Project SelectedProject
    {
        get => _selectedProject;
        set => RaiseAndSetIfChanged(ref _selectedProject, value);
    }

    public AvaloniaList<Project> FoundProjects { get; } = new();

    public int SelectedPatternSearch
    {
        get => _selectedPatternSearch;
        set => RaiseAndSetIfChanged(ref _selectedPatternSearch, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool IsHomePage => Page is WelcomePage;
        
    private bool IsNoEditors
    {
        get
        {
            if (ModuleManager.Editors.Count != 0) return false;

            MessageBox.Show(WindowsManager.MainWindow,
                App.GetResource<string>("Warning"),
                App.GetResource<string>("NoEditors"), new List<MessageBoxButton>
                {
                    new()
                    {
                        Default = true,
                        Result = "OK",
                        Text = App.GetResource<string>("NoEditorsButton"),
                        IsKeyDown = true
                    }
                }, MessageBox.MessageBoxIcon.Warning);
                
            return true;
        }
    }

    #endregion

    public MainViewModel()
    {
        SynchronizationContext context = SynchronizationContext.Current;

        Thread thread = new(CheckBytesWritten) { IsBackground = true };
        App.Threads.Add(thread);

        thread.Start(context);

        this.WhenAnyValue(x => x.SearchText, x => x.SelectedPatternSearch)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Subscribe(_ => FindProjects());

        this.WhenAnyValue(x => x.RegulMenuItems)
            .Subscribe(_ => RaiseMenuItemsProperty());
        
        this.WhenAnyValue(x => x.SelectedTab)
            .Subscribe(DoSelectTab);
        this.WhenAnyValue(x => x.Page)
            .Subscribe(DoRaisePageProperty);
    }

    private void DoRaisePageProperty(object obj)
    {
        RaisePropertyChanged(nameof(IsHomePage));
    }

    private void DoSelectTab(PleasantTabItem? tab)
    {
        UserControl control = (UserControl)tab?.Content;
        control?.Focus();
    }

    private void CheckBytesWritten(object state)
    {
        SynchronizationContext? context = state as SynchronizationContext;

        while (true)
        {
            _bytesWritten.WaitOne();

            foreach (string file in Directory.EnumerateFiles(Path.Combine(RegulPaths.Cache, "OpenFiles")))
            {
                string content = File.ReadAllText(file);

                File.Delete(file);

                context?.Send(_ =>
                {
                    WindowsManager.MainWindow.Topmost = true;
                    WindowsManager.MainWindow.Topmost = false;
                    WindowsManager.MainWindow.Activate();

                    GetFilesFromArguments(content.Split('|'));

                    if (_fileArgs.Count != 0)
                        DropOpenFile(_fileArgs);
                }, "");
            }

            if (Directory.GetFiles(Path.Combine(RegulPaths.Cache, "OpenFiles")).Length == 0)
                Directory.Delete(Path.Combine(RegulPaths.Cache, "OpenFiles"));
        }
    }

    public async void CreateNewFile()
    {
        if (IsNoEditors) return;

        (Editor? editor, _) = await SelectingEditor.GetEditor(null, false);

        if (editor is { }) CreateTab(null, editor, null);
    }

    public async void OpenFile()
    {
        if (IsNoEditors) return;

        OpenFileDialog dialog = new() { Filters = new List<FileDialogFilter>() };
        //foreach (Editor item in ModuleManager.Editors)
        for (int i = 0; i < ModuleManager.Editors.Count; i++)
        {
            Editor? item = ModuleManager.Editors[i];

            //foreach (FileDialogFilter item1 in item.DialogFilters)
            for (int i1 = 0; i1 < item?.DialogFilters.Count; i1++)
            {
                FileDialogFilter item1 = item.DialogFilters[i1] ?? throw new NullReferenceException();
                dialog.Filters.Add(new FileDialogFilter { Name = item1.Name, Extensions = item1.Extensions });
            }
        }

        dialog.Filters.Add(
            new FileDialogFilter { Name = App.GetResource<string>("AllFiles"), Extensions = { "*" } });

        string[]? result = await dialog.ShowAsync(WindowsManager.MainWindow);

        if (result is { Length: > 0 })
        {
            string path = result.First();

            Editor? editor = await GetEditor(path);

            if (editor == null) return;

            Project project = new(editor.Id, path);

            CreateTab(path, editor, project);
        }
    }

    public void GetFilesFromArguments(string?[] args)
    {
        _fileArgs.Clear();

        //foreach (string arg in args)
        for (int i = 0; i < args.Length; i++)
        {
            string? arg = args[i];
#if DEBUG
            Logger.Instance.WriteLog(Log.Debug, arg);
#endif
            try
            {
                _ = new FileInfo(arg!);
                _fileArgs.Add(arg);
            }
            catch
            {
                // ignored
            }
        }
    }

    public async void DropOpenFile(IEnumerable<string?> files)
    {
        if (IsNoEditors) return;

        foreach (string? file in files)
        {
            Editor? editor = await GetEditor(file);
            if (editor is null || file is null) continue;
            
            Project project = new(editor.Id, file);

            CreateTab(file, editor, project);
        }
    }

    public void DropLoadModules(IEnumerable<string?> files)
    {
        string?[] enumerable = files as string?[] ?? files.ToArray();

        List<Module?> modules = new();
        List<string> copiedFiles = new();

        foreach (string? file in enumerable)
        {
            if (file is null) continue;
            
            if (file.Contains(".zip"))
            {
                try
                {
                    copiedFiles.AddRange(Extensions.ImprovedExtractToDirectoryWithList(file, RegulPaths.Modules));
                }
                catch
                {
                    WindowsManager.ShowNotification(App.GetResource<string>("FailedCopyModule"), NotificationType.Error);
                    return;
                }
            }
            else if (file.Contains(".dll"))
            {
                string path = Path.Combine(RegulPaths.Modules, Path.GetFileName(file));
                try
                {
                    File.Copy(file, path);
                }
                catch
                {
                    WindowsManager.ShowNotification(App.GetResource<string>("FailedCopyModule"), NotificationType.Error);
                    return;
                }

                copiedFiles.Add(path);
            }
        }
            

        for (int i = 0; i < copiedFiles.Count; i++)
        {
            string module = copiedFiles[i];

            try
            {
                Module? loadedModule = ModuleManager.InitializeModule(module);
                if (loadedModule != null)
                    modules.Add(loadedModule);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(Log.Error,
                    $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to load module] {e}");
                    
                WindowsManager.ShowError(App.GetResource<string>("FailedLoadModule") + $" {module}", e.ToString());
            }
        }
        //

        //foreach (ModuleSystem.Models.Module module in modules)
        for (int i = 0; i < modules.Count; i++)
        {
            Module module = modules[i] ?? throw new NullReferenceException();

            try
            {
                module.Source.Execute();
                module.ChangeLanguage(GeneralSettings.Instance.Language, App.ModulesLanguage);
                module.Source.CorrectlyInitialized = true;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(Log.Error,
                    $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to initialize module] {e}");
                    
                WindowsManager.ShowError(App.GetResource<string>("FailedInitModule") + $" {module.Source.Name}", e.ToString());
            }
        }

        WindowsManager.ShowNotification(App.GetResource<string>("ModulesUploaded"));
    }

    private void OpenProject()
    {
        if (!File.Exists(SelectedProject.Path))
        {
            WindowsManager.ShowError(App.GetResource<string>("FailedLoadFile"), null);

            GeneralSettings.Instance.Projects.Remove(SelectedProject);
            FindProjects();

            return;
        }

        Editor? editor = ModuleManager.Editors.FirstOrDefault(x => x?.Id == SelectedProject.IdEditor);

        if (editor is null)
        {
            WindowsManager.ShowError(App.GetResource<string>("FailedFindEditor") + $" {Path.GetFileName(SelectedProject.Path)}", null);

            return;
        }

        CreateTab(SelectedProject.Path, editor, SelectedProject);
    }

    private async Task<Editor?> GetEditor(string? file)
    {
        CorrespondingExtensionEditor? cE = GeneralSettings.Instance.CorrespondingExtensionEditors
            .FirstOrDefault(x => x.Extension == Path.GetExtension(file));

        Editor? editor;
        bool alwaysOpenWithAnEditor = false;

        if (cE != null)
        {
            editor = ModuleManager.Editors.FirstOrDefault(x => x?.Id == cE.IdEditor);

            if (editor == null)
                (editor, alwaysOpenWithAnEditor) = await SelectingEditor.GetEditor(Path.GetFileName(file));
        }
        else
        {
            (editor, alwaysOpenWithAnEditor) = await SelectingEditor.GetEditor(Path.GetFileName(file));
        }

        if (alwaysOpenWithAnEditor)
            GeneralSettings.Instance.CorrespondingExtensionEditors.Add(new CorrespondingExtensionEditor
            {
                Extension = Path.GetExtension(file)!,
                IdEditor = editor?.Id
            });

        return editor;
    }

    public void CreateTab(string? path, Editor ed, Project? project)
    {
        foreach (PleasantTabItem pleasantTabItem in TabItems)
        {
            IEditor tabEditor = (IEditor)pleasantTabItem.Content;

            if (tabEditor.FilePath != null && path != null && tabEditor.FilePath.Contains(path))
            {
                WindowsManager.ShowNotification(App.GetResource<string>("FileIsAlreadyOpen"), NotificationType.Information);

                return;
            }
        }

        try
        {
            IEditor editor = ed.CreatingAnEditor.Invoke();
            editor!.FilePath = path;

            PleasantTabItem tabItem = new()
            {
                Header = string.IsNullOrEmpty(path) ? App.GetResource<string>("NoName") : Path.GetFileName(path),
                Content = editor,
                IsClosable = true,
                Icon = App.GetResource<Geometry>(ed.IconKey!),
                CanBeDragged = true
            };
            if (!string.IsNullOrEmpty(path)) tabItem.SetValue(ToolTip.TipProperty, $"{tabItem.Header}\n{path}");
            tabItem.Closing += async (_, _) =>
            {
                if (tabItem.IsEditedIndicator)
                {
                    SavedResult result = await SaveFile(tabItem, true);

                    if (result is SavedResult.Success or SavedResult.Disclaimer)
                    {
                        ((IEditor)tabItem.Content).Release();
                        tabItem.CloseCore();
                    }
                    else return;
                }
                else
                {
                    ((IEditor)tabItem.Content).Release();
                    tabItem.CloseCore();
                }

                if (TabItems.Count == 0) Page = new WelcomePage();
            };

            editor.Load(path, tabItem, ed);
            editor.Project = project;

            if (project is not null)
            {
                Project? foundedProject = GeneralSettings.Instance.Projects.FirstOrDefault(x => x.Path == project.Path);

                if (foundedProject is not null)
                {
                    GeneralSettings.Instance.Projects.Remove(foundedProject);

                    GeneralSettings.Instance.Projects.Insert(0, project);
                }
            }

            TabItems.Add(tabItem);

            SearchText = string.Empty;
            FindProjects();

            if (Page is not EditorsPage)
                Page = new EditorsPage();
        }
        catch (Exception ex)
        {
            WindowsManager.ShowError(App.GetResource<string>("FailedOpenFile") + " " + Path.GetFileName(path), ex.ToString());
        }
    }

    private async void SaveFile()
    {
        if (await SaveFile(SelectedTab, false) == SavedResult.Success)
            WindowsManager.ShowNotification($"{App.GetResource<string>("FileSuccessfullySavedC")} {SelectedTab.Header}",
                NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    public async Task<SavedResult> SaveFile(PleasantTabItem tab, bool askUser)
    {
        IEditor editor = (IEditor)tab.Content;

        bool boolResult = true;

        if (askUser && tab.IsEditedIndicator)
        {
            string result = await MessageBox.Show(WindowsManager.MainWindow,
                App.GetResource<string>("Warning"),
                $"{App.GetResource<string>("WarningSaveFile")} {tab.Header}?", new List<MessageBoxButton>
                {
                    new()
                    {
                        Text = App.GetResource<string>("Yes"),
                        Default = true,
                        Result = "Yes"
                    },
                    new()
                    {
                        Text = App.GetResource<string>("No"),
                        Result = "No"
                    },
                    new()
                    {
                        Text = App.GetResource<string>("Cancel"),
                        Result = "Cancel"
                    }
                }, MessageBox.MessageBoxIcon.Warning);

            switch (result)
            {
                case "No":
                    return SavedResult.Disclaimer;
                case "Cancel":
                    boolResult = false;
                    break;
            }
        }

        if (boolResult)
        {
            if (editor.FilePath is null)
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Filters = new List<FileDialogFilter>(editor.Editor.DialogFilters)
                    {
                        new()
                        {
                            Name = App.GetResource<string>("AllFiles"),
                            Extensions = { "*" }
                        }
                    }
                };

                string? newPath = await saveFileDialog.ShowAsync(WindowsManager.MainWindow);

                if (newPath is null) return SavedResult.Cancel;

                editor.FilePath = newPath;
            }

            if (editor.FilePath is not null && !editor.Save(editor.FilePath))
            {
                WindowsManager.ShowNotification(App.GetResource<string>("FailedSave"), NotificationType.Error, TimeSpan.FromSeconds(3));

                return SavedResult.Error;
            }

            string filename = Path.GetFileName(editor.FilePath)!;

            tab.Header = filename;
            tab.SetValue(ToolTip.TipProperty, $"{filename}\n{editor.FilePath}");

            if (editor.Project is not null)
            {
                Project? project = GeneralSettings.Instance.Projects.FirstOrDefault(x => x.Path == editor.Project.Path);
                
                GeneralSettings.Instance.Projects.Remove(project);
            }

            editor.Project = new Project(editor.Id, editor.FilePath!);

            GeneralSettings.Instance.Projects.Insert(0, editor.Project);
        }

        if (boolResult) return SavedResult.Success;

        return SavedResult.Cancel;
    }

    private async void SaveAs()
    {
        SaveFileDialog dialog = new() { Filters = new List<FileDialogFilter>(((IEditor)SelectedTab.Content).Editor.DialogFilters) };
        dialog.Filters.Add(
            new FileDialogFilter { Name = App.GetResource<string>("AllFiles"), Extensions = { "*" } });

        string? result = await dialog.ShowAsync(WindowsManager.MainWindow);

        if (result is null) return;
        
        IEditor editor = (IEditor)SelectedTab.Content;

        Project project =
            GeneralSettings.Instance.Projects.FirstOrDefault(x => x.Path == editor.Project?.Path)!;
        GeneralSettings.Instance.Projects.Remove(project);
        
        editor.FilePath = result;

        if (await SaveFile(SelectedTab, false) == SavedResult.Success)
            WindowsManager.ShowNotification($"{App.GetResource<string>("FileSuccessfullySavedC")} {SelectedTab.Header}",
                NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    private async void SaveAll()
    {
        //foreach (PleasantTabItem tab in TabItems)
        for (int i = 0; i < TabItems.Count; i++)
            await SaveFile(TabItems[i], false);
            
        WindowsManager.ShowNotification(App.GetResource<string>("FilesSuccessfullySaved"), NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    private async void OpenSettings()
    {
        WindowsManager.MainWindow.CancelClose = true;

        await WindowsManager.CreateModalWindow<SettingsWindow>()?.Show(WindowsManager.MainWindow)!;

        WindowsManager.MainWindow.CancelClose = false;
    }

    private void OpenAbout()
    {
        WindowsManager.CreateModalWindow<About>(WindowsManager.MainWindow);
    }

    private void OpenLogsWindow()
    {
        WindowsManager.CreateWindow<LogsWindow>();
    }

    public void BackToHomePage()
    {
        Page = new WelcomePage();
    }

    public void GoToEditors()
    {
        Page = new EditorsPage();
    }

    private void OpenHashGenerator()
    {
        WindowsManager.CreateWindow<HashGenerator>();
    }

    public void FindProjects()
    {
        FoundProjects.Clear();
        
        if (string.IsNullOrEmpty(SearchText))
        {
            FoundProjects.AddRange(GeneralSettings.Instance.Projects);
            return;
        }

        switch (SelectedPatternSearch)
        {
            case 0:
                //foreach (Project project in GeneralSettings.Settings.Projects)
                for (int index = 0; index < GeneralSettings.Instance.Projects.Count; index++)
                {
                    Project project = GeneralSettings.Instance.Projects[index];
                    if (Path.GetFileNameWithoutExtension(project.Path)!.ToLower()
                        .Contains(SearchText.ToLower()))
                        FoundProjects.Add(project);
                }

                break;
            case 1:
                //foreach (Project project in GeneralSettings.Settings.Projects)
                for (int index = 0; index < GeneralSettings.Instance.Projects.Count; index++)
                {
                    Project project = GeneralSettings.Instance.Projects[index];
                    if (project.Path.ToLower().Contains(SearchText.ToLower()))
                        FoundProjects.Add(project);
                }

                break;
            case 2:
                //foreach (Project project in GeneralSettings.Settings.Projects)
                for (int index = 0; index < GeneralSettings.Instance.Projects.Count; index++)
                {
                    Project project = GeneralSettings.Instance.Projects[index];
                    Editor? editor = ModuleManager.GetEditorById(project.IdEditor);
                    if (editor is not null && editor.Name.ToLower().Contains(SearchText.ToLower()))
                        FoundProjects.Add(project);
                }

                break;
        }
    }

    private void ClearGc()
    {
        WindowsManager.ShowNotification(App.GetResource<string>("MemorySuccessfullyCleared"), 
            NotificationType.Success, TimeSpan.FromSeconds(3));

        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private void DeleteProject()
    {
        GeneralSettings.Instance.Projects.Remove(SelectedProject);
    }

    #region Initialize

    public void Initialize()
    {
        InitializeMenuItems();
        InitializeModules();

        FindProjects();

        Page = new WelcomePage();

        Logger.Instance.WriteLog(Log.Info, "A new program session has been successfully created!");

        App.CheckUpdate(false);

        if (_fileArgs.Count != 0)
            DropOpenFile(_fileArgs);
    }

    private void InitializeMenuItems()
    {
        RegulMenuItems = new AvaloniaList<IRegulObject?>
        {
            new RegulMenuItem("FileMenu", null)
            {
                Bindings =
                {
                    new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                        new DynamicResourceExtension("FileMenu"))
                },
                Items =
                {
                    new RegulMenuItem("Create", Command.Create(CreateNewFile), KeyGesture.Parse("Ctrl+N"))
                    {
                        KeyIcon = "NewFileIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("Create"))
                        }
                    },
                    new RegulMenuItem("Open", Command.Create(OpenFile), KeyGesture.Parse("Ctrl+O"))
                    {
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("Open"))
                        }
                    },
                    new RegulMenuItem("Save", Command.Create(SaveFile), KeyGesture.Parse("Ctrl+S"))
                    {
                        KeyIcon = "SaveIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("Save")),
                            new Binding(InputElement.IsEnabledProperty,
                                new Avalonia.Data.Binding("SelectedTab").Converter(ObjectConverters.IsNotNull))
                        }
                    },
                    new RegulMenuItem("SaveAs", Command.Create(SaveAs))
                    {
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("SaveAs")),
                            new Binding(InputElement.IsEnabledProperty,
                                new Avalonia.Data.Binding("SelectedTab").Converter(ObjectConverters.IsNotNull))
                        }
                    },
                    new RegulMenuItem("SaveAll", Command.Create(SaveAll))
                    {
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("SaveAll")),
                            new Binding(InputElement.IsEnabledProperty, new Avalonia.Data.Binding("TabItems.Count"))
                        }
                    },

                    new RegulSeparator("Separator1"),

                    new RegulMenuItem("BackToHomePage", Command.Create(BackToHomePage))
                    {
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("BackToTheHomePage")),
                            new Binding(InputElement.IsEnabledProperty, new Avalonia.Data.Binding("!IsHomePage"))
                        }
                    },
                    new RegulMenuItem("Settings", Command.Create(OpenSettings))
                    {
                        KeyIcon = "SettingsIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("Settings"))
                        }
                    },
                    new RegulMenuItem("ExitFromRegul", Command.Create(WindowsManager.MainWindow.Close),
                        KeyGesture.Parse("Ctrl+Q"))
                    {
                        KeyIcon = "ExitIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("ExitFromRegul"))
                        }
                    }
                }
            },
            new RegulMenuItem("ToolsMenu", null)
            {
                Bindings =
                {
                    new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                        new DynamicResourceExtension("ToolsMenu"))
                },
                Items =
                {
                    new RegulMenuItem("HashGenerator", Command.Create(OpenHashGenerator))
                    {
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("HashGenerator"))
                        }
                    }
                }
            },
            new RegulMenuItem("ReferenceMenu", null)
            {
                Bindings =
                {
                    new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                        new DynamicResourceExtension("ReferenceMenu"))
                },
                Items =
                {
                    new RegulMenuItem("CheckForUpdates", Command.Create(() => { App.CheckUpdate(true); }))
                    {
                        KeyIcon = "UpdateIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("CheckForUpdates"))
                        }
                    },

                    new RegulSeparator("Separator1"),

                    new RegulMenuItem("OpenLogsWindow", Command.Create(OpenLogsWindow))
                    {
                        KeyIcon = "MonitorIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("OpenLogsWindow"))
                        }
                    },

                    new RegulSeparator("Separator2"),

                    new RegulMenuItem("AboutTheProgram", Command.Create(OpenAbout))
                    {
                        KeyIcon = "RegulAltIcon",
                        Bindings =
                        {
                            new Binding(HeaderedSelectingItemsControl.HeaderProperty,
                                new DynamicResourceExtension("AboutTheProgram"))
                        }
                    }
                }
            }
        };
    }

    private void InitializeModules()
    {
        if (!Directory.Exists(RegulPaths.Modules))
            Directory.CreateDirectory(RegulPaths.Modules);

        if (!Directory.Exists(RegulPaths.Cache))
            Directory.CreateDirectory(RegulPaths.Cache);

        bool isSuccessfullyUpdateModules = false;

        foreach (ModuleForUpdate moduleForUpdate in GeneralSettings.Instance.ModulesForUpdate)
        {
            if (!File.Exists(moduleForUpdate.Path)) continue;

            try
            {
                Extensions.ImprovedExtractToDirectory(moduleForUpdate.Path,
                    Path.GetDirectoryName(moduleForUpdate.PathToModule)!);
                isSuccessfullyUpdateModules = true;
            }
            finally
            {
                File.Delete(moduleForUpdate.Path);
            }
        }

        GeneralSettings.Instance.ModulesForUpdate.Clear();

        foreach (string module in
                 Directory.EnumerateFiles(RegulPaths.Modules, "*.dll", SearchOption.AllDirectories))
            try
            {
                ModuleManager.InitializeModule(module);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(Log.Error,
                    $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to load module] {e}");

                WindowsManager.ShowError(App.GetResource<string>("FailedLoadModule") + $" {module}", e.ToString());
            }

        //foreach (ModuleSystem.Models.Module module in ModuleManager.System.Modules)
        for (int i = 0; i < ModuleManager.Modules.Count; i++)
        {
            Module module = ModuleManager.Modules[i];

            try
            {
                module.Source.Execute();
                module.ChangeLanguage(GeneralSettings.Instance.Language, App.ModulesLanguage);
                module.Source.CorrectlyInitialized = true;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(Log.Error,
                    $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to initialize module] {e}");

                WindowsManager.ShowError(App.GetResource<string>("FailedInitModule") + $" {module.Source.Name}", e.ToString());
            }
        }

        if (isSuccessfullyUpdateModules)
            WindowsManager.ShowNotification(App.GetResource<string>("ModulesSuccessfullyUpdated"));
    }

    #endregion

    #region Updates

    public void RaiseIsHomePageProperty()
    {
        RaisePropertyChanged(nameof(IsHomePage));
    }

    public void RaiseMenuItemsProperty()
    {
        RaisePropertyChanged(nameof(MenuItems));
    }

    #endregion
}