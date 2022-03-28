using System;
using System.Collections.Generic;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using Onebeld.Extensions;
using Onebeld.Logging;

namespace Regul.Base.Views.Windows;

public class LogsWindowViewModel : ViewModelBase
{
    private TextDocument _textDocument = new();

    public LogsWindowViewModel()
    {
        //foreach (string item in Logger.Current.Logs)
        for (int i = 0; i < Logger.Instance.Logs.Count; i++)
        {
            string item = Logger.Instance.Logs[i];
            if (Logger.Instance.Logs.IndexOf(item) == 0)
                TextDocument.Text += item;
            else
                TextDocument.Text += "\n" + item;
        }

        Logger.Instance.Written += CurrentWritten;
    }

    public TextDocument TextDocument
    {
        get => _textDocument;
        set => RaiseAndSetIfChanged(ref _textDocument, value);
    }

    private void CurrentWritten(object sender, EventArgs e)
    {
        if (Logger.Instance.Logs.IndexOf((string)sender) == 0)
            TextDocument.Text += (string)sender;
        else
            TextDocument.Text += "\n" + (string)sender;
    }

    private void ClearLogs()
    {
        Logger.Instance.Logs.Clear();
        TextDocument.Text = string.Empty;
    }

    private async void SaveLogs()
    {
        SaveFileDialog saveFileDialog = new()
        {
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Log-" + App.GetResource<string>("Files"), Extensions = { "log" } }
            }
        };

        string? path = await saveFileDialog.ShowAsync(WindowsManager.FindWindow<LogsWindow>()!);

        if (!string.IsNullOrEmpty(path))
            Logger.Instance.SaveLog(path);
    }
}