#region

using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

#endregion

namespace Regul.Base.Views.Windows
{
    public class NewProject : PleasantDialogWindow
    {
        public NewProject()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}