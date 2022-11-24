using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using Regul.Managers;
using Regul.ModuleSystem.Structures;
using Regul.ViewModels.Windows;

namespace Regul.Views.Windows;

public class EditorSelectionWindow : ContentDialog
{
    public EditorSelectionWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static async Task<(Editor?, bool)> GetEditor(string? fileName, bool showCheckBox = true)
    {
        EditorSelectionWindow? editorSelectionWindow = WindowsManager.CreateModalWindow<EditorSelectionWindow>();
        if (editorSelectionWindow is null)
            return (null, false);

        editorSelectionWindow.DataContext = new EditorSelectionViewModel();

        TextBlock? textBlock = editorSelectionWindow.FindControl<TextBlock>("PART_FileName");
        CheckBox? checkBox = editorSelectionWindow.FindControl<CheckBox>("PART_AlwaysOpen");
        
        if (textBlock is not null)
        {
            textBlock.IsVisible = fileName is not null;
            textBlock.Text = fileName;
        }
        if (checkBox is not null)
            checkBox.IsVisible = showCheckBox;
        
        editorSelectionWindow.FindControl<ListBox>("PART_ListBox")?.Focus();

        Editor? editor = await editorSelectionWindow.Show<Editor?>(WindowsManager.MainWindow);

        return (editor, editor != null && (checkBox?.IsChecked ?? false));
    }
}