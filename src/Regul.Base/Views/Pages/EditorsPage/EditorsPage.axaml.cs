#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.Base.Views.Windows;

#endregion

namespace Regul.Base.Views.Pages
{
    public class EditorsPage : UserControl
    {
        public EditorsPage()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<PleasantTabView>("PART_PleasantTabView").ClickOnAddingButton += (s, e) =>
            {
                NewProject newProject = new NewProject();
                WindowsManager.OtherModalWindows.Add(newProject);

                newProject.Show(WindowsManager.MainWindow);
            };
        }
    }
}