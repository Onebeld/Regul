using System;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows;

public class LogsWindow : PleasantWindow
{
    public LogsWindow()
    {
        InitializeComponent();

        Closed += LogsWindow_Closed;
    }

    private void LogsWindow_Closed(object? sender, EventArgs e)
    {
        WindowsManager.OtherWindows.Remove(this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}