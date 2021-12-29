using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Onebeld.Extensions;
using Onebeld.Logging;
using PleasantUI.Controls.Custom;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Base.Other;
using Regul.Base.Views.Pages;
using Regul.ModuleSystem;
using Path = Avalonia.Controls.Shapes.Path;

namespace Regul.Base.Views.Windows
{
    public class MainViewModel : ViewModelBase
    {
        private IManagedNotificationManager _notificationManager;
        private object _page;

        private AvaloniaList<IAvaloniaObject> _menuItems;
        private AvaloniaList<PleasantTabItem> _tabItems = new AvaloniaList<PleasantTabItem>();
        private PleasantTabItem _selectedTab;
        private Project _selectedProject;

        private int _selectedPatternSearch;
        private string _searchText;

        private AvaloniaList<Project> _foundProjects;

        #region Properties

        public IManagedNotificationManager NotificationManager
        {
            get => _notificationManager;
            set => RaiseAndSetIfChanged(ref _notificationManager, value);
        }

        public object Page
        {
            get => _page;
            set
            {
                RaiseAndSetIfChanged(ref _page, value);
                RaisePropertyChanged("IsHomePage");
            }
        }

        public AvaloniaList<IAvaloniaObject> MenuItems
        {
            get => _menuItems;
            set => RaiseAndSetIfChanged(ref _menuItems, value);
        }

        public AvaloniaList<PleasantTabItem> TabItems
        {
            get => _tabItems;
            set => RaiseAndSetIfChanged(ref _tabItems, value);
        }

        public PleasantTabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                RaiseAndSetIfChanged(ref _selectedTab, value);

                if (value != null)
                {
                    UserControl control = (UserControl)value.Content;
                    control?.Focus();
                }
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set => RaiseAndSetIfChanged(ref _selectedProject, value);
        }

        public AvaloniaList<Project> FoundProjects
        {
            get => _foundProjects;
            set => RaiseAndSetIfChanged(ref _foundProjects, value);
        }

        private int SelectedPatternSearch
        {
            get => _selectedPatternSearch;
            set
            {
                RaiseAndSetIfChanged(ref _selectedPatternSearch, value);
                FindProjects();
            }
        }

        private string SearchText
        {
            get => _searchText;
            set
            {
                RaiseAndSetIfChanged(ref _searchText, value);
                FindProjects();
            }
        }

        public bool IsHomePage
        {
            get => Page is WelcomePage;
        }

        #endregion

        #region Initialize

        public void Initialize()
        {
            if (GeneralSettings.Settings.Projects == null)
                GeneralSettings.Settings.Projects = new AvaloniaList<Project>();

            InitializeMenuItems();
            InitializeModules();

            FindProjects();

            Page = new WelcomePage();

            Logger.Current.WriteLog(Log.Info, "A new program session has been successfully created!");

            App.CheckUpdate(false);
        }

        private void InitializeMenuItems()
        {
            MenuItems = new AvaloniaList<IAvaloniaObject>
            {
                new MenuItem
                {
                    Items = new AvaloniaList<IAvaloniaObject>
                    {
                        new MenuItem
                        {
                            Command = Command.Create(CreateNewFile),
                            InputGesture = KeyGesture.Parse("Ctrl+N"),
                            HotKey = KeyGesture.Parse("Ctrl+N"),
                            Icon = new Path {Data = App.GetResource<Geometry>("NewFileIcon")}
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("Create")),
                        new MenuItem
                        {
                            Command = Command.Create(OpenFile),
                            InputGesture = KeyGesture.Parse("Ctrl+O"),
                            HotKey = KeyGesture.Parse("Ctrl+O")
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("Open")),
                        new MenuItem
                            {
                                Command = Command.Create(SaveFile),
                                InputGesture = KeyGesture.Parse("Ctrl+S"),
                                HotKey = KeyGesture.Parse("Ctrl+S"),
                                Icon = new Path {Data = App.GetResource<Geometry>("SaveIcon")}
                            }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("Save"))
                            .Bind<MenuItem>(InputElement.IsEnabledProperty, new Binding("SelectedTab").Converter(ObjectConverters.IsNotNull)),
                        new MenuItem
                            {
                                Command = Command.Create(SaveAs)
                            }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("SaveAs"))
                            .Bind<MenuItem>(InputElement.IsEnabledProperty,
                                new Binding("SelectedTab").Converter(ObjectConverters.IsNotNull)),
                        new MenuItem
                        {
                            Command = Command.Create(SaveAll)
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("SaveAll"))
                        .Bind<MenuItem>(InputElement.IsEnabledProperty, new Binding("TabItems.Count")),
                        new Separator(),
                        new MenuItem
                        {
                            Command = Command.Create(BackToHomePage)
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("BackToTheHomePage"))
                        .Bind<MenuItem>(InputElement.IsEnabledProperty, new Binding("!IsHomePage")),
                        new MenuItem
                        {
                            Command = Command.Create(OpenSettings),
                            Icon = new Path {Data = App.GetResource<Geometry>("SettingsIcon")}
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("Settings")),
                        new MenuItem
                        {
                            Command = Command.Create(() => { WindowsManager.MainWindow.Close(); }),
                            InputGesture = KeyGesture.Parse("Ctrl+Q"),
                            Icon = new Path {Data = App.GetResource<Geometry>("ExitIcon")}
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("ExitFromRegul"))
                    }
                }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("FileMenu")),

                new MenuItem
                {
                    Items = new AvaloniaList<IAvaloniaObject>
                    {
                        new MenuItem
                        {
                            Command = Command.Create(OpenHashGenerator)
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("HashGenerator"))
                    }
                }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("ToolsMenu")),

                new MenuItem
                {
                    Items = new AvaloniaList<IAvaloniaObject>
                    {
                        new MenuItem
                        {
                            Command = Command.Create(() => { App.CheckUpdate(true); }),
                            Icon = new Path {Data = App.GetResource<Geometry>("UpdateIcon")}
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("CheckForUpdates")),
                        new MenuItem
						{
                            Command = Command.Create(() => { OpenLogsWindow(); })
						}.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("OpenLogsWindow")),
                        new MenuItem
                        {
                            Command = Command.Create(OpenAbout),
                            Icon = new Path {Data = App.GetResource<Geometry>("RegulIcon")}
                        }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("AboutTheProgram"))
                    }
                }.Bind<MenuItem>(MenuItem.HeaderProperty, new DynamicResourceExtension("ReferenceMenu"))
            };
        }
        private void InitializeModules()
        {
            if (!Directory.Exists(CorePaths.Modules))
                Directory.CreateDirectory(CorePaths.Modules);

            foreach (string module in Directory.EnumerateFiles(CorePaths.Modules, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    ModuleManager.InitializeModule(module);
                }
                catch (Exception e)
                {
                    Logger.Current.WriteLog(Log.Error, $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to load module] {e}");

                    MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                        App.GetResource<string>("FailedLoadModule") + $" {module}", new List<MessageBoxButton>
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

            foreach (IModule module in ModuleManager.System.Modules)
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
                        App.GetResource<string>("FailedInitModule") + $" {module.Name}",
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
            }
        }

        #endregion

        public void UpdateProperties()
		{
            RaisePropertyChanged("IsHomePage");
        }

        public async void CreateNewFile()
        {
            if (IsNoEditors()) return;

            Editor editor = await SelectingEditor.GetEditor();

            if (editor != null) CreateTab(null, editor, null);
        }

        public async void OpenFile()
        {
            if (IsNoEditors()) return;

            Editor editor = await SelectingEditor.GetEditor();

            if (editor == null) return;

            OpenFileDialog dialog = new OpenFileDialog { Filters = editor.DialogFilters };
            dialog.Filters.Add(new FileDialogFilter { Name = App.GetResource<string>("AllFiles"), Extensions = { "*" } });

            string[] result = await dialog.ShowAsync(WindowsManager.MainWindow);

            if (result != null && result.Length > 0)
            {
                string path = result.First();

                Project project = new Project
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    EditorName = editor.Name,
                    EditorIcon = editor.Icon.ToString()
                };

                CreateTab(path, editor, project);
            }
        }

        private void OpenProject()
        {
            if (!File.Exists(SelectedProject.Path))
            {
                MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                    App.GetResource<string>("FailedLoadFile"), new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            Default = true,
                            Result = "OK",
                            Text = App.GetResource<string>("OK"),
                            IsKeyDown = true
                        }
                    }, MessageBox.MessageBoxIcon.Error);

                GeneralSettings.Settings.Projects.Remove(SelectedProject);
                FindProjects();

                return;
            }

            Editor editor = null;

            for (int index = 0; index < ModuleManager.Editors.Count; index++)
            {
                Editor editor1 = ModuleManager.Editors[index];
                if (editor1.Name == SelectedProject.EditorName)
                {
                    editor = editor1;
                    break;
                }
            }

            if (editor == null)
            {
                MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                    App.GetResource<string>("FailedFindEditor") + $" {SelectedProject.EditorName}", new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            Default = true,
                            Result = "OK",
                            Text = App.GetResource<string>("OK"),
                            IsKeyDown = true
                        }
                    }, MessageBox.MessageBoxIcon.Error);

                return;
            }

            CreateTab(SelectedProject.Path, editor, SelectedProject);
        }

        public void CreateTab(string path, Editor ed, Project project)
        {
            IEditor editor = ed.CreatingAnEditor.Invoke();
            editor.FileToPath = path;

            if (project != null)
            {
                GeneralSettings.Settings.Projects.Remove(project);
                GeneralSettings.Settings.Projects.Insert(0, project);
            }

            PleasantTabItem tabItem = new PleasantTabItem
            {
                Header = string.IsNullOrEmpty(path) ? App.GetResource<string>("NoName") : System.IO.Path.GetFileNameWithoutExtension(path),
                Content = editor,
                IsClosable = true,
                Icon = ed.Icon,
                CanBeDragged = true
            };
            tabItem.Closing += async (s, e) =>
            {
                if (tabItem.IsEditedIndicator)
                {
                    SavedResult result = await SaveFile(tabItem, true);

                    if (result == SavedResult.Success || result == SavedResult.Disclaimer)
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

                if (TabItems.Count == 0)
                    Page = new WelcomePage();
            };

            editor.Load(path, project, tabItem, ed);

            TabItems.Add(tabItem);

            SearchText = string.Empty;
            FindProjects();

            if (!(Page is EditorsPage))
                Page = new EditorsPage();
        }

        public bool IsNoEditors()
        {
            if (ModuleManager.Editors.Count == 0)
            {
                MessageBox.Show(WindowsManager.MainWindow,
                    App.GetResource<string>("Warning"),
                    App.GetResource<string>("NoEditors"), new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            Default = true,
                            Result = "OK",
                            Text = App.GetResource<string>("NoEditorsButton"),
                            IsKeyDown = true
                        }
                    }, MessageBox.MessageBoxIcon.Warning);

                return true;
            }

            return false;
        }

        public async void SaveFile()
        {
            if (await SaveFile(SelectedTab, false) == SavedResult.Success)
                NotificationManager.Show(new Notification(App.GetResource<string>("Successful"), $"{App.GetResource<string>("FileSuccessfullySavedC")} {SelectedTab.Header}", NotificationType.Success, TimeSpan.FromSeconds(3)));
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
                        new MessageBoxButton
                        {
                            Text = App.GetResource<string>("Yes"),
                            Default = true,
                            Result = "Yes"
                        },
                        new MessageBoxButton
                        {
                            Text = App.GetResource<string>("No"),
                            Result = "No"
                        },
                        new MessageBoxButton
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
                if (editor.FileToPath == null)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog { Filters = editor.CurrentEditor.DialogFilters };
                    saveFileDialog.Filters.Add(new FileDialogFilter { Name = App.GetResource<string>("AllFiles"), Extensions = { "*" } });

                    string newPath = await saveFileDialog.ShowAsync(WindowsManager.MainWindow);

                    if (string.IsNullOrEmpty(newPath)) return SavedResult.Cancel;

                    editor.FileToPath = newPath;
                }

                if (!editor.Save(editor.FileToPath))
                {
                    NotificationManager.Show(new Notification(App.GetResource<string>("Error"), App.GetResource<string>("FailedSave"), NotificationType.Error, TimeSpan.FromSeconds(3)));

                    return SavedResult.Error;
                }

                string filename = System.IO.Path.GetFileNameWithoutExtension(editor.FileToPath);

                tab.Header = filename;

                if (editor.CurrentProject != null)
                {
                    for (int i = 0; i < GeneralSettings.Settings.Projects.Count; i++)
                    {
                        Project project = GeneralSettings.Settings.Projects[i];
                        if (project.Path == editor.CurrentProject.Path)
                        {
                            GeneralSettings.Settings.Projects.Remove(project);
                            break;
                        }
                    }
                }

                editor.CurrentProject = new Project
                {
                    Name = filename,
                    Path = editor.FileToPath,
                    EditorName = editor.CurrentEditor.Name,
                    EditorIcon = editor.CurrentEditor.Icon.ToString()
                };

                GeneralSettings.Settings.Projects.Insert(0, editor.CurrentProject);
            }

            if (boolResult) return SavedResult.Success;

            return SavedResult.Cancel;
        }

        public async void SaveAs()
        {
			SaveFileDialog dialog = new SaveFileDialog { Filters = new List<FileDialogFilter>(((IEditor)SelectedTab.Content).CurrentEditor.DialogFilters) };
            dialog.Filters.Add(new FileDialogFilter { Name = App.GetResource<string>("AllFiles"), Extensions = { "*" } });

            string result = await dialog.ShowAsync(WindowsManager.MainWindow);

            if (!string.IsNullOrEmpty(result))
            {
                IEditor editor = (IEditor)SelectedTab.Content;

                if (editor.CurrentProject != null)
                {
                    foreach (Project project in GeneralSettings.Settings.Projects)
                    {
                        if (project.Path == editor.CurrentProject.Path)
                        {
                            GeneralSettings.Settings.Projects.Remove(project);
                            break;
                        }
                    }
                }

                editor.FileToPath = result;

                if (await SaveFile(SelectedTab, false) == SavedResult.Success)
                	NotificationManager.Show(new Notification(App.GetResource<string>("Successful"), $"{App.GetResource<string>("FileSuccessfullySavedC")} {SelectedTab.Header}", NotificationType.Success, TimeSpan.FromSeconds(3)));
            }
        }

        public async void SaveAll()
        {
            for (int index = 0; index < TabItems.Count; index++)
                await SaveFile(TabItems[index], false);

            NotificationManager.Show(new Notification(App.GetResource<string>("Successful"), App.GetResource<string>("FilesSuccessfullySaved"), NotificationType.Success, TimeSpan.FromSeconds(3)));
        }

        private async void OpenSettings()
        {
            if (WindowsManager.SettingsWindow != null && WindowsManager.SettingsWindow.CanOpen)
                return;

            WindowsManager.MainWindow.CancelClose = true;
            WindowsManager.SettingsWindow = new SettingsWindow();

            await WindowsManager.SettingsWindow.Show(WindowsManager.MainWindow);

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
            HashGenerator generator = new HashGenerator();

            WindowsManager.OtherWindows.Add(generator);

            generator.Show();
        }

        public void FindProjects()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                FoundProjects = GeneralSettings.Settings.Projects;
                return;
            }

            FoundProjects = new AvaloniaList<Project>();

            switch (SelectedPatternSearch)
            {
                case 0:
                    foreach (Project project in GeneralSettings.Settings.Projects)
                    {
                        if (project.Name.ToLower().Contains(SearchText.ToLower()))
                            FoundProjects.Add(project);
                    }
                    break;
                case 1:
                    foreach (Project project in GeneralSettings.Settings.Projects)
                    {
                        if (project.Path.ToLower().Contains(SearchText.ToLower()))
                            FoundProjects.Add(project);
                    }
                    break;
                case 2:
                    foreach (Project project in GeneralSettings.Settings.Projects)
                    {
                        if (project.EditorName.ToLower().Contains(SearchText.ToLower()))
                            FoundProjects.Add(project);
                    }
                    break;
            }
        }

        private void ClearGC()
        {
            GC.Collect();

            NotificationManager.Show(new Notification(App.GetResource<string>("Successful"), App.GetResource<string>("MemorySuccessfullyCleared"), NotificationType.Success, TimeSpan.FromSeconds(3)));
        }

        private void DeleteProject() => GeneralSettings.Settings.Projects.Remove(SelectedProject);
    }
}