﻿using System.Collections.Specialized;
using Avalonia.Collections;
using PleasantUI;
using PleasantUI.Extensions;
using PleasantUI.Reactive;
using Regul.Managers;
using Regul.ModuleSystem;
using Regul.Structures;
using Regul.Views.Windows;

namespace Regul.ViewModels.Windows;

public class OpenProjectViewModel : ViewModelBase
{
    private readonly OpenProjectWindow _openProjectWindow;

    private string _searchName = string.Empty;
    private string _searchPath = string.Empty;
    private string _searchEditor = string.Empty;

    private bool _reverseProjectList;
    private bool _sortByAlphabetical;
    private bool _sortByDateOfChange = true;

    public AvaloniaList<Project> SortedProjects { get; } = new();

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

    public OpenProjectViewModel(OpenProjectWindow openProjectWindow)
    {
        _openProjectWindow = openProjectWindow;

        OnSearch(ApplicationSettings.Current.Projects);
        ApplicationSettings.Current.Projects.CollectionChanged += ProjectsOnCollectionChanged;

        this.WhenAnyValue(x => x.SearchName, x => x.SearchPath)
            .Skip(1)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));
        this.WhenAnyValue(x => x.SearchEditor)
            .Skip(1)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));
        this.WhenAnyValue(x => x.SortByAlphabetical, x => x.SortByDateOfChange)
            .Skip(1)
            .Where(x => x.Item1 || x.Item2)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));
        this.WhenAnyValue(x => x.ReverseProjectList)
            .Skip(1)
            .Subscribe(_ => OnSearch(ApplicationSettings.Current.Projects));
    }

    private void ProjectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnSearch(ApplicationSettings.Current.Projects);

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

    public void OpenProject(Project project)
    {
        WindowsManager.MainWindow?.ViewModel.OpenProject(project);

        _openProjectWindow.Close();
    }

    public void DeleteProject(Project project) => ApplicationSettings.Current.Projects.Remove(project);

    public void Close() => _openProjectWindow.Close();
}