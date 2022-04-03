using System;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows;

public class LogsWindow : PleasantWindow
{
    public LogsWindow() => AvaloniaXamlLoader.Load(this);
}