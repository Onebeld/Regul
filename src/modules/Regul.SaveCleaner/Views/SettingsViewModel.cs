using System;
using Avalonia.Controls;
using Onebeld.Extensions;
using Regul.Base;

namespace Regul.SaveCleaner.Views
{
    public class SettingsViewModel : ViewModelBase
    {
        private void CloseWindow()
        {
            SettingsWindow foundSettings = WindowsManager.FindModalWindow<SettingsWindow>();
            
            foundSettings?.Close();

            WindowsManager.OtherModalWindows.Remove(foundSettings);
        }
        
        private void Define()
        {
            SaveCleanerSettings.Settings.PathToTheSims3Document = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Electronic Arts\\The Sims 3";
            SaveCleanerSettings.Settings.PathToSaves = SaveCleanerSettings.Settings.PathToTheSims3Document + "\\Saves";
        }

        private async void ChoosePath()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string path = await dialog.ShowAsync(WindowsManager.MainWindow);
            if (!string.IsNullOrEmpty(path))
            {
                SaveCleanerSettings.Settings.PathToTheSims3Document = path;
                SaveCleanerSettings.Settings.PathToSaves = SaveCleanerSettings.Settings.PathToTheSims3Document + "\\Saves";
            }
        }
    }
}