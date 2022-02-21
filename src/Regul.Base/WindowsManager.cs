using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using PleasantUI.Controls.Custom;
using Regul.Base.Views.Windows;

namespace Regul.Base
{
    public static class WindowsManager
    {
        public static MainWindow MainWindow { get; set; }

        public static AvaloniaList<PleasantModalWindow> OtherModalWindows { get; set; } =
            new AvaloniaList<PleasantModalWindow>();

        public static AvaloniaList<PleasantWindow> OtherWindows { get; set; } = new AvaloniaList<PleasantWindow>();

        public static T FindModalWindow<T>() where T : PleasantModalWindow
        {
			//return (T)OtherModalWindows.FirstOrDefault(x => x is T);
			for (int i = 0; i < OtherModalWindows.Count; i++)
			{
				PleasantModalWindow item = OtherModalWindows[i];

                if (item is T window)
                    return window;
			}
            return null;
        }

        public static T FindWindow<T>() where T : PleasantWindow
        {
            //return (T) OtherWindows.FirstOrDefault(x => x is T);
            for (int i = 0; i < OtherWindows.Count; i++)
            {
                PleasantWindow item = OtherWindows[i];

                if (item is T window)
                    return window;
            }
            return null;
        }

        public static List<T> FindAllModalWindows<T>() where T : PleasantModalWindow
        {
            List<T> otherWindows = new List<T>();

            //foreach (PleasantDialogWindow window in OtherModalWindows)
			for (int i = 0; i < OtherModalWindows.Count; i++)
            {
					PleasantDialogWindow window = (PleasantDialogWindow)OtherModalWindows[i];
					if (window is T pleasantModalWindow)
                        otherWindows.Add(pleasantModalWindow);
            }

            return otherWindows;
        }
        public static List<T> FindAllWindows<T>() where T : PleasantWindow
        {
            List<T> otherWindows = new List<T>();

			//foreach (PleasantWindow window in OtherWindows)
			for (int i = 0; i < OtherWindows.Count; i++)
            {
				PleasantWindow window = OtherWindows[i];

				if (window is T pleasantWindow)
                    otherWindows.Add(pleasantWindow);
            }

            return otherWindows;
        }

        public static T CreateModalWindow<T>(PleasantWindow host = null, params object[] args) where T : PleasantModalWindow
		{
            T foundWindow = FindModalWindow<T>();

            if (foundWindow != null && foundWindow.CanOpen) return null;

            T window = (T)Activator.CreateInstance(typeof(T), args);
            OtherModalWindows.Add(window);

            if (host != null)
                window.Show(host);

            return window;
        }

        public static T CreateWindow<T>(PleasantWindow host = null, params object[] args) where T : PleasantWindow
		{
            T foundWindow = FindWindow<T>();

            if (foundWindow != null) return null;

            T window = (T)Activator.CreateInstance(typeof(T), args);
            OtherWindows.Add(window);

            if (host != null)
                window.Show(host);
            else
                window.Show();

            return window;
		}

        public static T GetDataContext<T>(this Control control)
        {
            return (T)control.DataContext;
        }
    }
}