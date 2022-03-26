using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Onebeld.Extensions;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows
{
    public class AboutViewModel : ViewModelBase
    {
        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        public string FrameworkVersion => RuntimeInformation.FrameworkDescription;
        public string PleasantUiVersion => Assembly.GetAssembly(typeof(PleasantWindow))?.GetName().Version?.ToString();

        public void Close()
        {
            About window = WindowsManager.FindModalWindow<About>();

            WindowsManager.OtherModalWindows.Remove(window);

            window.Close();
        }

        private void GoToGitHub()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Onebeld/Regul",
                UseShellExecute = true
            });
        }

        private void GoToPatreon()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.patreon.com/onebeld",
                UseShellExecute = true
            });
        }
    }
}