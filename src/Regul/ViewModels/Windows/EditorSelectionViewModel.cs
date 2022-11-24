using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Extensions;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Structures;

namespace Regul.ViewModels.Windows;

public class EditorSelectionViewModel : ViewModelBase
{
    private Editor? _selectedEditor;

    private bool _invertEditorList;
    private string _editorNameSearching = string.Empty;

    public AvaloniaList<Editor> SortedEditors { get; } = new();

    public Editor? SelectedEditor
    {
        get => _selectedEditor;
        set => RaiseAndSetIfChanged(ref _selectedEditor, value);
    }

    public bool InvertEditorList
    {
        get => _invertEditorList;
        set => RaiseAndSetIfChanged(ref _invertEditorList, value);
    }
    public string EditorNameSearching
    {
        get => _editorNameSearching;
        set => RaiseAndSetIfChanged(ref _editorNameSearching, value);
    }

    public EditorSelectionViewModel()
    {
        this.WhenAnyValue(x => x.EditorNameSearching, x => x.InvertEditorList)
            .Subscribe(_ => OnSearchEditors(ModuleManager.Editors));
    }
    
    private void OnSearchEditors(AvaloniaList<Editor> editors)
    {
        SortedEditors.Clear();

        List<Editor> list = new(editors);
        
        if (!string.IsNullOrWhiteSpace(EditorNameSearching))
            list = list.FindAll(x =>
            {
                if (x.Name is null) return false;
                
                if (Application.Current is not null && Application.Current.TryFindResource(x.Name, out object? name) && name is string s)
                {
                    return s.ToLower().Contains(EditorNameSearching);
                }

                return x.Name.ToLower().Contains(EditorNameSearching);
            });
        
        if (InvertEditorList)
            SortedEditors.AddRange(list.OrderByDescending(x => x.Name));
        else SortedEditors.AddRange(list.OrderBy(x => x.Name));
    }

    public void CloseWithEditor(ContentDialog contentDialog)
    {
        contentDialog.Close(SelectedEditor);
    }

    public void Close(ContentDialog contentDialog)
    {
        contentDialog.Close(null);
    }
}