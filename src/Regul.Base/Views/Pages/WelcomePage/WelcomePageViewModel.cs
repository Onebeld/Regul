using Onebeld.Extensions;
using Regul.Base.Views.Windows;

namespace Regul.Base.Views.Pages
{
    public class WelcomePageViewModel : ViewModelBase
    {
        public bool ThereAreOpenEditors
        {
            get 
            {
                MainViewModel viewModel = WindowsManager.MainWindow.GetDataContext<MainViewModel>();

                return viewModel.TabItems.Count > 0;
            }
        }

        private void GoTo()
        {
            MainViewModel viewModel = WindowsManager.MainWindow.GetDataContext<MainViewModel>();

            viewModel.Page = new EditorsPage();
			viewModel.UpdateProperties();
        }
    }
}