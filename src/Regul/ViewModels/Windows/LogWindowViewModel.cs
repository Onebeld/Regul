using System;
using System.Collections.Generic;
using Avalonia.Controls;
using PleasantUI;
using PleasantUI.Controls;
using Regul.Logging;

#pragma warning disable CS0618

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
        SaveFileDialog saveFileDialog = new()
        {
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Log-" + App.GetString("Files"),
                    Extensions =
                    {
                        "log"
                    }
                }
            },
            DefaultExtension = "log"
        };

        string? path = await saveFileDialog.ShowAsync(_window);

        if (!string.IsNullOrEmpty(path))
            Logger.Instance.SaveLogsToFile(path);
    }
}