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

        public static async Task<Editor> GetEditor()
		{
            SelectingEditor foundEditor = WindowsManager.FindModalWindow<SelectingEditor>();

            if (foundEditor != null && foundEditor.CanOpen)
                return null;

            SelectingEditor selectEditor = new SelectingEditor();
            selectEditor.GetDataContext<SelectingEditorViewModel>().Editors = ModuleManager.Editors;
            WindowsManager.OtherModalWindows.Add(selectEditor);
            Editor editor = await selectEditor.Show<Editor>(WindowsManager.MainWindow);
            WindowsManager.OtherModalWindows.Remove(selectEditor);

            return editor;
        }
    }
}