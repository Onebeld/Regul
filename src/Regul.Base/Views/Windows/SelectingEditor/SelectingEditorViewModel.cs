using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Collections;
using Onebeld.Extensions;
using Regul.ModuleSystem;

namespace Regul.Base.Views.Windows;

public class SelectingEditorViewModel : ViewModelBase
{
    private string _searchText = string.Empty;
    private Editor? _selectedItem;
    
    #region Propeties

    public Editor? SelectedItem
    {
        get => _selectedItem;
        set => RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public AvaloniaList<Editor?> FoundEditors { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    #endregion

    public SelectingEditorViewModel()
    {
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Subscribe(_ => FindEditor());
    }

    public void CloseWithSave()
    {
        WindowsManager.FindModalWindow<SelectingEditor>()?.Close(SelectedItem);
    }

    public void Close()
    {
        WindowsManager.FindModalWindow<SelectingEditor>()?.Close(null);
    }

    private void FindEditor()
    {
        FoundEditors.Clear();
        
        if (string.IsNullOrEmpty(SearchText))
        {
            FoundEditors.AddRange(ModuleManager.Editors);
            return;
        }

        foreach (Editor? editor in ModuleManager.Editors)
        {
            if (editor is not null && editor.Name.ToLower().Contains(SearchText.ToLower()))
                FoundEditors.Add(editor);
        }
    }
}