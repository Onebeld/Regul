using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using PleasantUI.Controls.Custom;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Base.Views.Windows;

namespace Regul.Base;

public static class WindowsManager
{
    public static MainWindow MainWindow { get; set; } = null!;

    public static AvaloniaList<PleasantModalWindow?> OtherModalWindows { get; } = new();

    public static AvaloniaList<PleasantWindow?> OtherWindows { get; } = new();

    public static T? FindModalWindow<T>() where T : PleasantModalWindow
    {
        //return (T)OtherModalWindows.FirstOrDefault(x => x is T);
        for (int i = 0; i < OtherModalWindows.Count; i++)
        {
            PleasantModalWindow? item = OtherModalWindows[i];

            if (item is T window)
                return window;
        }

        return null;
    }

    public static T? FindWindow<T>() where T : PleasantWindow
    {
        //return (T) OtherWindows.FirstOrDefault(x => x is T);
        for (int i = 0; i < OtherWindows.Count; i++)
        {
            PleasantWindow? item = OtherWindows[i];

            if (item is T window)
                return window;
        }

        return null;
    }

    public static List<T> FindAllModalWindows<T>() where T : PleasantModalWindow
    {
        List<T> otherWindows = new();

        //foreach (PleasantDialogWindow window in OtherModalWindows)
        for (int i = 0; i < OtherModalWindows.Count; i++)
        {
            PleasantDialogWindow window = (PleasantDialogWindow)OtherModalWindows[i]!;
            if (window is T pleasantModalWindow)
                otherWindows.Add(pleasantModalWindow);
        }

        return otherWindows;
    }

    public static List<T> FindAllWindows<T>() where T : PleasantWindow
    {
        List<T> otherWindows = new();

        //foreach (PleasantWindow window in OtherWindows)
        for (int i = 0; i < OtherWindows.Count; i++)
        {
            PleasantWindow? window = OtherWindows[i];

            if (window is T pleasantWindow)
                otherWindows.Add(pleasantWindow);
        }

        return otherWindows;
    }

    public static T? CreateModalWindow<T>(PleasantWindow? host = null, params object[] args)
        where T : PleasantModalWindow
    {
        T? foundWindow = FindModalWindow<T>();

        if (foundWindow is { CanOpen: true }) return null;

        T window = (T)Activator.CreateInstance(typeof(T), args);
        OtherModalWindows.Add(window);

        if (host != null)
            window.Show(host);

        return window;
    }

    public static T? CreateWindow<T>(PleasantWindow? host = null, params object[] args) where T : PleasantWindow
    {
        T? foundWindow = FindWindow<T>();

        if (foundWindow != null) return null;

        T window = (T)Activator.CreateInstance(typeof(T), args);
        OtherWindows.Add(window);

        if (host != null)
            window.Show(host);
        else
            window.Show();

        return window;
    }

    public static T GetDataContext<T>(this Control? control)
    {
        return (T)control?.DataContext!;
    }
        
    public static void ShowError(string? message, string? exception)
    {
        MessageBox.Show(MainWindow, App.GetResource<string>("Error"), message, new List<MessageBoxButton>
        {
            new()
            {
                Default = true,
                Result = "OK",
                Text = App.GetResource<string>("OK"),
                IsKeyDown = true
            }
        }, MessageBox.MessageBoxIcon.Error, exception);
    }

    public static void ShowNotification(string? message, NotificationType type = NotificationType.Success, TimeSpan expiration = default)
    {
        string? title;
            
        switch (type)
        {
            case NotificationType.Information:
                title = App.GetResource<string>("Error");
                break;
            case NotificationType.Warning:
                title = App.GetResource<string>("Warning");
                break;
            case NotificationType.Error:
                title = App.GetResource<string>("Error");
                break;
                
            case NotificationType.Success:
            default:
                title = App.GetResource<string>("Successful");
                break;
        }
            
        if (expiration == default)
            expiration = TimeSpan.FromSeconds(5);
            
        MainWindow.GetDataContext<MainViewModel>().NotificationManager.Show(new Notification(title, message, type, expiration));
    }
}