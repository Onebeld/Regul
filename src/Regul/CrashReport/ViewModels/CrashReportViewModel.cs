using Avalonia;
using Avalonia.Platform.Storage;
using PleasantUI;
using PleasantUI.Controls;
using Regul.Logging;

namespace Regul.CrashReport.ViewModels;

public class CrashReportViewModel : ViewModelBase
{
    public string ExceptionText { get; set; }

    public CrashReportViewModel(string exceptionText) => ExceptionText = exceptionText;

    public void CopyLogs() => Application.Current?.Clipboard?.SetTextAsync(ExceptionText);

    public async void SaveLogs(PleasantWindow window)
    {
        IStorageFile? file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new []
            {
                new FilePickerFileType("Log " + App.GetString("Files"))
                {
                    Patterns = new[] { ".log" }
                }
            },
            DefaultExtension = ".log"
        });
        
        if (file is not null)
            Logger.Instance.SaveLogsToFile(file.Path.AbsolutePath);
    }

    public void Close(PleasantWindow window) => window.Close();

    public void CloseAndRelaunch(PleasantWindow window)
    {
        ApplicationSettings.Current.RestartingApp = true;
        window.Close();
    }
}