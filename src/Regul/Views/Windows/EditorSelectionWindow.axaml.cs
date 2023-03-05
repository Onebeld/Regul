using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using Regul.Managers;
using Regul.ModuleSystem.Structures;
using Regul.ViewModels.Windows;

namespace Regul.Views.Windows;

public class EditorSelectionWindow : ContentDialog
{
    public EditorSelectionWindow() => AvaloniaXamlLoader.Load(this);

    public static async Task<(Editor?, bool)> GetEditor(string? fileName, bool showCheckBox = true)
    {
        if (WindowsManager.MainWindow is null) return (null, false);

        EditorSelectionWindow? editorSelectionWindow = WindowsManager.CreateModalWindow<EditorSelectionWindow>();
        if (editorSelectionWindow is null)
            return (null, false);

        editorSelectionWindow.DataContext = new EditorSelectionViewModel();

        TextBlock? textBlock = editorSelectionWindow.FindControl<TextBlock>("FileName");
        CheckBox? checkBox = editorSelectionWindow.FindControl<CheckBox>("AlwaysOpen");

        if (textBlock is not null)
        {
            textBlock.IsVisible = fileName is not null;
            textBlock.Text = fileName;
        }
        if (checkBox is not null)
            checkBox.IsVisible = showCheckBox;

        editorSelectionWindow.FindControl<ListBox>("ListBox")?.Focus();

        Editor? editor = await editorSelectionWindow.Show<Editor?>(WindowsManager.MainWindow);

        return (editor, editor != null && (checkBox?.IsChecked ?? false));
    }
}