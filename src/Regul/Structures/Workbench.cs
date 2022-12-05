using PleasantUI;
using Regul.Interfaces;

namespace Regul.Structures;

public class Workbench : ViewModelBase
{
    private string? _pathToFile;
    private bool _isDirty;
    private IEditorViewModel? _editorViewModel;
    private string _editorId;

    /// <summary>
    /// Open file name.
    /// </summary>
    public string? PathToFile
    {
        get => _pathToFile;
        set => RaiseAndSetIfChanged(ref _pathToFile, value);
    }

    /// <summary>
    /// Indicates whether the current file has been changed.
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        set => RaiseAndSetIfChanged(ref _isDirty, value);
    }
    /// <summary>
    /// Editor's logic. It should be a ViewModel.
    /// </summary>
    public IEditorViewModel? EditorViewModel
    {
        get => _editorViewModel;
        set => RaiseAndSetIfChanged(ref _editorViewModel, value);
    }

    public string EditorId
    {
        get => _editorId;
        set => RaiseAndSetIfChanged(ref _editorId, value);
    }
}