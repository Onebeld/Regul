using Avalonia.Collections;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Extensions;
using PleasantUI.Reactive;
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
            list = list.FindAll(x => x.Name is not null && App.GetString(x.Name).ToLower().Contains(EditorNameSearching));

        list = new List<Editor>(list.OrderBy(x => x.Name));

        if (InvertEditorList)
            list.Reverse();

        SortedEditors.AddRange(list);
    }

    public void CloseWithEditor(ContentDialog contentDialog) => contentDialog.Close(SelectedEditor);

    public void Close(ContentDialog contentDialog) => contentDialog.Close(null);
}