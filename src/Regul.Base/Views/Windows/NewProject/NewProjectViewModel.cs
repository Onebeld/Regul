using System.Collections.Generic;
using System.IO;
using Avalonia.Collections;
using Onebeld.Extensions;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.ModuleSystem;

namespace Regul.Base.Views.Windows
{
    public class NewProjectViewModel : ViewModelBase
    {
        private Project _selectedProject;
        private AvaloniaList<Project> _foundProjects;
        
        private int _selectedPatternSearch;
        private string _searchText;

        #region Propetries

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
        
        public int SelectedPatternSearch
        {
            get => _selectedPatternSearch;
            set
            {
                RaiseAndSetIfChanged(ref _selectedPatternSearch, value); 
                SearchProjects();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                RaiseAndSetIfChanged(ref _searchText, value);
                SearchProjects();
            }
        }

        #endregion

        public NewProjectViewModel()
        {
            SearchProjects();
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
                SearchProjects();
                
                return;
            }

            Editor ed = null;

            MainViewModel viewModel = WindowsManager.MainWindow.GetDataContext<MainViewModel>();

            foreach (Editor editor1 in ModuleManager.Editors)
            {
                if (editor1.Name == SelectedProject.EditorName)
                {
                    ed = editor1;
                    break;
                }
            }

            if (ed == null)
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

            viewModel.CreateTab(SelectedProject.Path, ed, SelectedProject);
            
            CloseWindow();
        }
        
        private void CreateNew()
        {
            WindowsManager.MainWindow.GetDataContext<MainViewModel>().CreateNewFile();
            
            CloseWindow();
        }

        private void OpenFile()
        {
            WindowsManager.MainWindow.GetDataContext<MainViewModel>().OpenFile();
            
            CloseWindow();
        }
        
        private void SearchProjects()
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

        private void CloseWindow()
        {
            NewProject foundNewProject = WindowsManager.FindModalWindow<NewProject>();
            
            foundNewProject?.Close();

            WindowsManager.OtherModalWindows.Remove(foundNewProject);
            
            WindowsManager.MainWindow.GetDataContext<MainViewModel>().FindProjects();
        }
        
        private void DeleteProject() => GeneralSettings.Settings.Projects.Remove(SelectedProject);
    }
}