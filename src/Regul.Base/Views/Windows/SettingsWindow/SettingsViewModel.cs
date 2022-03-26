using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
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
using Regul.Base.Other;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Models;
using Regul.Settings;
using Extensions = Regul.Base.Other.Extensions;
using Module = Regul.ModuleSystem.Models.Module;

namespace Regul.Base.Views.Windows
{
    public class SettingsViewModel : ViewModelBase
    {
        private Theme _customTheme;
        private AvaloniaList<Theme> _customThemes = new AvaloniaList<Theme>();

        private bool _isChecksForUpdates;

        private readonly Loading _loading = new Loading();

        private AvaloniaList<Module> _modules = new AvaloniaList<Module>();
        private Language _selectedLanguage;
        private Module _selectedModule;
        private int _selectedTheme;

        private SynchronizationContext _synchronizationContext;
        private bool _thereAreUpdatesForModules;
        
        #region Properties

        // The index is used, because there is a problem when loading the settings window, when the current theme is not selected
        public int SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                RaiseAndSetIfChanged(ref _selectedTheme, value);

                if (value == -1) return;
                CustomTheme = null;

                DefaultTheme defaultTheme = (DefaultTheme)Themes[value].DataContext;

                GeneralSettings.Instance.Theme = defaultTheme.Name;

                Application.Current.Styles[1] = new StyleInclude(new Uri("resm:Style?assembly=Regul"))
                {
                    Source = new Uri($"avares://PleasantUI/Assets/Themes/{defaultTheme.Name}.axaml")
                };
            }
        }

        public AvaloniaList<Theme> CustomThemes
        {
            get => _customThemes;
            set => RaiseAndSetIfChanged(ref _customThemes, value);
        }

        public Theme CustomTheme
        {
            get => _customTheme;
            set
            {
                RaiseAndSetIfChanged(ref _customTheme, value);

                if (value == null) return;

                SelectedTheme = -1;

                GeneralSettings.Instance.Theme = value.Name;

                Application.Current.Styles[1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(value.ToAxaml());
            }
        }

        public Language SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                RaiseAndSetIfChanged(ref _selectedLanguage, value);

                if (GeneralSettings.Instance.Language == value.Key)
                    return;

                GeneralSettings.Instance.Language = value.Key;

                Application.Current.Styles[3] = new StyleInclude(new Uri("resm:Style?assembly=Regul"))
                {
                    Source = new Uri($"avares://Regul.Assets/Localization/{value.Key}.axaml")
                };

                //foreach (IModule module in ModuleManager.System.Modules)
                for (int i = 0; i < ModuleManager.Modules.Count; i++)
                    ModuleManager.Modules[i]
                        .ChangeLanguage(GeneralSettings.Instance.Language, App.ModulesLanguage);
            }
        }

        public Module SelectedModule
        {
            get => _selectedModule;
            set => RaiseAndSetIfChanged(ref _selectedModule, value);
        }

        public AvaloniaList<Module> Modules
        {
            get => _modules;
            set => RaiseAndSetIfChanged(ref _modules, value);
        }

        public List<Language> Languages =>
            //return App.Languages.ToList();
            new List<Language>(App.Languages);

        public List<ComboBoxItem> Themes
        {
            get
            {
                List<ComboBoxItem> themes = new List<ComboBoxItem>();

                foreach (DefaultTheme item in PleasantUIDefaults.Themes)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Bind<ComboBoxItem>(ContentControl.ContentProperty,
                        new DynamicResourceExtension(item.Name));
                    comboBoxItem.DataContext = item;

                    themes.Add(comboBoxItem);
                }

                return themes;
            }
        }

        public bool IsChecksForUpdates
        {
            get => _isChecksForUpdates;
            set => RaiseAndSetIfChanged(ref _isChecksForUpdates, value);
        }

        public bool ThereAreUpdatesForModules
        {
            get => _thereAreUpdatesForModules;
            set => RaiseAndSetIfChanged(ref _thereAreUpdatesForModules, value);
        }

        #endregion

        private void Close()
        {
            SettingsWindow settingsWindow = WindowsManager.FindModalWindow<SettingsWindow>();

            settingsWindow.Close();
            WindowsManager.OtherModalWindows.Remove(settingsWindow);
        }

        public void Initialize()
        {
            _synchronizationContext = SynchronizationContext.Current;
            ThereAreUpdatesForModules = ModuleManager.Modules.Any(x => x.Source.ThereIsAnUpdate);

            LoadThemes();

            GetModules();

            SelectedTheme =
                Themes.FindIndex(x => ((DefaultTheme)x.DataContext)?.Name == GeneralSettings.Instance.Theme);
            if (SelectedTheme == -1)
            {
                //Theme theme = CustomThemes.FirstOrDefault(t => t.Name == GeneralSettings.Settings.Theme);
                Theme theme = null;
                for (int i = 0; i < CustomThemes.Count; i++)
                {
                    Theme item = CustomThemes[i];

                    if (item.Name == GeneralSettings.Instance.Theme)
                    {
                        theme = item;
                        break;
                    }
                }

                if (theme != null)
                    CustomTheme = theme;
                else SelectedTheme = 0;
            }

            //SelectedLanguage = Languages.FirstOrDefault(x => x.Key == GeneralSettings.Settings.Language)
            //	?? App.Languages.First(x => x.Key == "en");
            for (int i = 0; i < Languages.Count; i++)
            {
                Language item = Languages[i];
                if (item.Key == GeneralSettings.Instance.Language)
                {
                    SelectedLanguage = item;
                    break;
                }
            }

            if (SelectedLanguage == null)
                for (int i = 0; i < Languages.Count; i++)
                {
                    Language item = Languages[i];
                    if (item.Key == "en")
                    {
                        SelectedLanguage = item;
                        break;
                    }
                }
            //
        }

        private void GetModules()
        {
            //foreach (ModuleSystem.Models.Module module in ModuleManager.System.Modules) Modules.Add(module);
            for (int i = 0; i < ModuleManager.Modules.Count; i++)
                Modules.Add(ModuleManager.Modules[i]);
        }

        public void Release()
        {
            if (!Directory.Exists(RegulPaths.Themes))
                Directory.CreateDirectory(RegulPaths.Themes);

            foreach (string path in Directory.EnumerateFiles(RegulPaths.Themes)) File.Delete(path);

            for (int index = 0; index < CustomThemes.Count; index++)
            {
                Theme customTheme = CustomThemes[index];

                foreach (DefaultTheme item in PleasantUIDefaults.Themes)
                    if (item.Name == customTheme.Name)
                        customTheme.Name += " New";


                //if (CustomThemes.Any(x => x.Name == customTheme.Name))
                //	customTheme.Name += " New";
                int count = 0;
                while (true)
                {
                    Theme item = CustomThemes[count];

                    if (!item.Equals(customTheme) && item.Name == customTheme.Name)
                    {
                        customTheme.Name += " New";
                        count = 0;
                        continue;
                    }

                    count++;
                    if (count >= CustomThemes.Count)
                        break;
                }
                //

                if (!string.IsNullOrEmpty(customTheme.Name))
                    using (FileStream stream = File.Create(RegulPaths.Themes + $"/{customTheme.Name}.xml"))
                    {
                        new XmlSerializer(typeof(Theme)).Serialize(stream, customTheme);
                    }
            }

            if (CustomTheme != null)
                GeneralSettings.Instance.Theme = CustomTheme.Name;
        }

        private void LoadThemes()
        {
            if (!Directory.Exists("Themes")) return;

            foreach (string path in Directory.EnumerateFiles(RegulPaths.Themes, "*.xml"))
                using (FileStream fs = File.OpenRead(path))
                {
                    Theme theme = (Theme)new XmlSerializer(typeof(Theme)).Deserialize(fs);

                    CustomThemes.Add(theme);
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
                        Extensions = { "dll" }
                    },
                    new FileDialogFilter
                    {
                        Name = $"ZIP-{App.GetResource<string>("Files")}",
                        Extensions = { "zip" }
                    }
                }
            };

            string[] result = await dialog.ShowAsync(WindowsManager.MainWindow);

            if (result == null || result.Length <= 0) return;

            List<Module> modules = new List<Module>();
            List<string> copiedFiles = new List<string>();

            foreach (string file in result)
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

            foreach (string copiedFile in copiedFiles)
                try
                {
                    Module loadedModule = ModuleManager.InitializeModule(copiedFile);
                    if (loadedModule != null)
                        modules.Add(loadedModule);
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteLog(Log.Error,
                        $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] [Failed to load module] {e}");

                    WindowsManager.ShowError(App.GetResource<string>("FailedLoadModule") + $" {copiedFile}", e.ToString());
                }

            //foreach (ModuleSystem.Models.Module module in modules)
            for (int i = 0; i < modules.Count; i++)
            {
                Module module = modules[i];
                if (ModuleManager.Modules.Contains(module))
                    continue;

                InitializeModule(module, false);
            }

            UpdateModulesList();

            WindowsManager.ShowNotification(App.GetResource<string>("ModulesUploaded"));
        }

        private void InitializeModule(Module module, bool updateList = true)
        {
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

                WindowsManager.ShowError(App.GetResource<string>("FailedInitModule") + $" {SelectedModule.Source.Name}", e.ToString());
            }

            if (updateList)
                UpdateModulesList();
        }

        private void XamlInitializeModule(Module module)
        {
            InitializeModule(module);
        }

        private void DeleteCorrespondingExtensionEditor(CorrespondingExtensionEditor ce)
        {
            GeneralSettings.Instance.CorrespondingExtensionEditors.Remove(ce);
        }

        private void UpdateModulesList()
        {
            int index = ModuleManager.Modules.IndexOf(SelectedModule);

            Modules.Clear();
            GetModules();

            if (index != -1)
                //SelectedModule = Modules.ElementAt(index);

                for (int i = 0; i < Modules.Count; i++)
                    if (i == index)
                        SelectedModule = Modules[i];
        }

        private void CreateTheme()
        {
            Theme theme = new Theme
            {
                Name = App.GetResource<string>("NoName")
            };

            // So far, this is the most convenient way to create a theme, just going through almost all the properties of a new theme
            foreach (PropertyInfo property in typeof(Theme).GetProperties())
            {
                if (property.Name == "Name")
                    continue;

                property.SetValue(theme, App.GetResource<Color>(property.Name).ToUint32());
            }

            CustomThemes.Add(theme);
        }

        private async void ChangeColor(Button button)
        {
            try
            {
                IBrush brush = (await WindowColorPicker.SelectColor(WindowsManager.MainWindow,
                    ((ISolidColorBrush)button.Background)?.ToString())).ToBursh();

                button.Background = brush;

                Application.Current.Styles[1] = (IStyle)AvaloniaRuntimeXamlLoader.Load(CustomTheme.ToAxaml());
            }
            catch (Exception ex)
            {
                if (!(ex is TaskCanceledException))
                    Logger.Instance.WriteLog(Log.Error,
                        $"[{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}()] [Failed to change the color of the theme] {ex}");
            }
        }

        private void DeleteTheme()
        {
            CustomThemes.Remove(CustomTheme);

            SelectedTheme = 0;
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
                Logger.Instance.WriteLog(Log.Error,
                    $"[{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}()] [I couldn't insert the theme] {ex}");
            }
        }

        private void CopyColor(Button button)
        {
            Application.Current.Clipboard.SetTextAsync(
                ColorHelpers.ToHexColor(((SolidColorBrush)button.Background).Color));
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

        #region UpdateModule

        private void XamlUpdateModule(Module module)
        {
            _loading.Show(WindowsManager.MainWindow);

            UpdateModule(module);

            _loading.Close();
            UpdateModulesList();

            MainViewModel viewModel = WindowsManager.MainWindow.GetDataContext<MainViewModel>();
            viewModel.NotificationManager.Show(new Notification(App.GetResource<string>("Information"),
                App.GetResource<string>("NextTimeModulesUpdate")));
        }

        private async void UpdateModule(Module module)
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (!Directory.Exists(RegulPaths.Cache))
                        Directory.CreateDirectory(RegulPaths.Cache);

                    Worker_ProgressChanged(0, $"{module.Source.Name}\n{App.GetResource<string>("PreparationM")}", true);

                    string zipFile = Path.Combine(RegulPaths.Cache, module.Source.Name + ".zip");
                    string pathToModule = module.PluginLoader.PluginConfig.MainAssemblyPath;

                    if (File.Exists(zipFile))
                    {
                        GeneralSettings.Instance.ModulesForUpdate.Add(new ModuleForUpdate
                        {
                            Path = zipFile,
                            PathToModule = pathToModule
                        });

                        module.InfoForUpdate.ReadyForUpdate = true;

                        return;
                    }

                    WebClient webClient = new WebClient();

                    webClient.DownloadProgressChanged += (s, e1) =>
                    {
                        _synchronizationContext.Send(async (obj) =>
                        {
                            _loading.ProgressBar.IsIndeterminate = false;

                            _loading.ProgressBar.Value = e1.ProgressPercentage;
                            _loading.TextBlock.Text =
                                $"{module.Source.Name}\n{App.GetResource<string>("DownloadingM")}";
                        }, "");
                    };

                    await webClient.DownloadFileTaskAsync(new Uri(module.InfoForUpdate.LinkForDownload), zipFile);

                    webClient.Dispose();

                    GeneralSettings.Instance.ModulesForUpdate.Add(new ModuleForUpdate
                    {
                        Path = zipFile,
                        PathToModule = pathToModule
                    });

                    module.InfoForUpdate.ReadyForUpdate = true;
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog(Log.Error, ex.ToString());
            }
        }

        private void UpdateModules()
        {
            _loading.Show(WindowsManager.MainWindow);

            foreach (Module module in ModuleManager.Modules)
            {
                if (module.InfoForUpdate.ReadyForUpdate)
                    continue;

                UpdateModule(module);
            }

            _loading.Close();
            UpdateModulesList();
            
            WindowsManager.ShowNotification(App.GetResource<string>("NextTimeModulesUpdate"), NotificationType.Information);
        }

        private void Worker_ProgressChanged(int progress, string userState, bool isIndeterminate)
        {
            _synchronizationContext.Send(state =>
            {
                _loading.ProgressBar.IsIndeterminate = isIndeterminate;
                _loading.ProgressBar.Value = progress;
                _loading.TextBlock.Text = userState;
            }, "");
        }

        private async void CheckUpdateModule()
        {
            IsChecksForUpdates = true;

            await Task.Run(() => { Parallel.ForEach(ModuleManager.Modules, module => { module.CheckUpdate(); }); });

            IsChecksForUpdates = false;

            UpdateModulesList();

            if (!ModuleManager.Modules.Any(x => x.Source.ThereIsAnUpdate))
            {
                WindowsManager.ShowNotification(App.GetResource<string>("UpdatesForModulesNotFound"), NotificationType.Information);
            }
            else
            {
                ThereAreUpdatesForModules = true;
                WindowsManager.ShowNotification(App.GetResource<string>("UpdatesForModulesAvailable"), NotificationType.Information);
            }
        }

        #endregion
    }
}