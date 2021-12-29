using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.S3PE.Windows.Editors
{
    public class STBLEditor : PleasantDialogWindow
    {
        public AvaloniaEdit.TextEditor AvaloniaEdit;
        
        public STBLEditor()
        {
            AvaloniaXamlLoader.Load(this);

            AvaloniaEdit = this.FindControl<AvaloniaEdit.TextEditor>("PART_AvaloniaEdit");
        }
    }
}