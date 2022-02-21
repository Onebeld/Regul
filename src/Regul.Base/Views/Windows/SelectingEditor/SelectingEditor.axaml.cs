using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.ModuleSystem;
using System.Threading.Tasks;

namespace Regul.Base.Views.Windows
{
    public class SelectingEditor : PleasantDialogWindow
    {
        public SelectingEditor()
        {
            AvaloniaXamlLoader.Load(this);

            TemplateApplied += (s, e) =>
            {
                this.GetDataContext<SelectingEditorViewModel>().Initialize();
            };
        }

        public static async Task<(Editor, bool)> GetEditor(string fileName, bool showCheckBox = true)
		{
            SelectingEditor selectingEditor = WindowsManager.CreateModalWindow<SelectingEditor>();

            if (selectingEditor == null)
                return (null, false);

            selectingEditor.FindControl<TextBlock>("PART_FileName").Text = fileName;

            CheckBox checkBox = selectingEditor.FindControl<CheckBox>("PART_AlwaysOpen");
            checkBox.IsVisible = showCheckBox;

            selectingEditor.GetDataContext<SelectingEditorViewModel>().Editors = ModuleManager.Editors;

            Editor editor = await selectingEditor.Show<Editor>(WindowsManager.MainWindow);
            WindowsManager.OtherModalWindows.Remove(selectingEditor);

            return (editor, editor == null ? false : checkBox.IsChecked ?? false);
        }
    }
}