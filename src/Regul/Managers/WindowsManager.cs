using System;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using PleasantUI.Controls;
using Regul.Views;

namespace Regul.Managers;

public static class WindowsManager
{
    public static MainWindow? MainWindow { get; internal set; }

    public static AvaloniaList<ContentDialog> ModalWindows { get; } = new();

    public static AvaloniaList<Window> Windows { get; } = new();

    public static T? FindModalWindow<T>() where T : ContentDialog =>
        (T?)ModalWindows.FirstOrDefault(x => x is T);

    public static T? FindMiniWindow<T>() where T : PleasantMiniWindow =>
        (T?)Windows.FirstOrDefault(x => x is T);

    public static T? CreateModalWindow<T>(PleasantWindow? host = null, params object[] args) where T : ContentDialog
    {
        T? foundWindow = FindModalWindow<T>();

        if (foundWindow is { CanOpen: true }) return null;

        T window = (T)Activator.CreateInstance(typeof(T), args)!;
        window.Closed += (sender, _) =>
        {
            if (sender is ContentDialog dialog)
                ModalWindows.Remove(dialog);
        };
        ModalWindows.Add(window);

        if (host != null)
            window.Show(host);

        return window;
    }

    public static T? CreateMiniWindow<T>(PleasantWindow? host = null, params object[] args) where T : PleasantMiniWindow
    {
        T? foundWindow = FindMiniWindow<T>();

        if (foundWindow is not null) return null;

        T window = (T)Activator.CreateInstance(typeof(T), args)!;
        window.Closed += (sender, _) =>
        {
            if (sender is PleasantMiniWindow miniWindow)
                Windows.Remove(miniWindow);
        };
        Windows.Add(window);

        if (host != null)
            window.Show(host);

        return window;
    }

    public static PleasantMiniWindow CreateMiniWindow(PleasantWindow? host = null)
    {
        PleasantMiniWindow window = new();
        window.Closed += (sender, _) =>
        {
            Windows.Remove((sender as PleasantMiniWindow)!);
        };
        Windows.Add(window);

        if (host != null)
            window.Show(host);

        return window;
    }
}