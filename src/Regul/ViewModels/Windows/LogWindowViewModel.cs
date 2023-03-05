using Avalonia.Platform.Storage;
using PleasantUI;
using PleasantUI.Controls;
using Regul.Logging;

namespace Regul.ViewModels.Windows;

public class LogWindowViewModel : ViewModelBase
{
    private readonly PleasantMiniWindow _window;
    private string _logs = string.Empty;

    public string Logs
    {
        get => _logs;
        set => RaiseAndSetIfChanged(ref _logs, value);
    }

    public LogWindowViewModel(PleasantMiniWindow window)
    {
        _window = window;

        foreach (string log in Logger.Instance.Logs)
        {
            if (Logger.Instance.Logs.IndexOf(log) == 0)
                Logs += log;
            else Logs += "\n" + log;
        }

        Logger.Instance.WrittenLog += OnWrittenLog;

        window.Closing += (_, _) => Logger.Instance.WrittenLog -= OnWrittenLog;
    }
    private void OnWrittenLog(object? sender, EventArgs e)
    {
        string log = (sender as string)!;

        if (Logger.Instance.Logs.IndexOf(log) == 0)
            Logs += log;
        else Logs += "\n" + log;
    }

    public void ClearLogs()
    {
        Logger.Instance.Logs.Clear();
        Logs = string.Empty;
    }

    public async void SaveLogs()
    {
        IStorageFile? file = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
}