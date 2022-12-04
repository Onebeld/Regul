using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using PleasantUI;
using PleasantUI.Controls;
using Regul.Logging;

#pragma warning disable CS0618

namespace Regul.CrashReport.ViewModels;

public class CrashReportViewModel : ViewModelBase
{
    public string ExceptionText { get; set; }

    public CrashReportViewModel(string exceptionText) => ExceptionText = exceptionText;

    public void CopyLogs() => Application.Current?.Clipboard?.SetTextAsync(ExceptionText);

    public async void SaveLogs(PleasantWindow window)
    {
        SaveFileDialog saveFileDialog = new()
        {
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Log " + App.GetString("Files"),
                    Extensions =
                    {
                        "log"
                    }
                }
            },
            DefaultExtension = "log"
        };

        string? path = await saveFileDialog.ShowAsync(window);

        if (!string.IsNullOrEmpty(path))
            Logger.Instance.SaveLogsToFile(path);
    }

    public void Close(PleasantWindow window) => window.Close();

    public void CloseAndRelaunch(PleasantWindow window)
    {
        ApplicationSettings.Current.RestartingApp = true;
        window.Close();
    }
}