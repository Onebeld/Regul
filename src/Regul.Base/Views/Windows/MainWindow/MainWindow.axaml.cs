using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PleasantUI;
using PleasantUI.Controls.Custom;
using Regul.Base.DragAndDrop;
using Regul.Base.Other;

namespace Regul.Base.Views.Windows;

public class MainWindow : PleasantWindow
{
#if DEBUG
    private bool _canGetAnException;
#endif
    private bool _closing;
    private bool _isDragging;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        this.AttachDevTools();

        MainViewModel viewModel = new();

        DataContext = viewModel;

        TemplateApplied += (s, e) =>
        {
            MainViewModel? viewModel1 = this.GetDataContext<MainViewModel>();

            viewModel1.Initialize();
        };

        viewModel.NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };

#if DEBUG
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.LeftShift)
                _canGetAnException = true;
        };

        KeyUp += (s, e) =>
        {
            if (e.Key == Key.LeftShift)
                _canGetAnException = false;
        };
#endif


        Closing += async (s, e) =>
        {
            e.Cancel = true;

            if (!_closing && !CancelClose)
            {
                GeneralSettings.Save();
                PleasantSettings.Save();

                for (int i = 0; i < viewModel.TabItems.Count; i++)
                {
                    PleasantTabItem tabItem = viewModel.TabItems[i];
                    if (!tabItem.IsEditedIndicator) continue;

                    viewModel.SelectedTab = tabItem;
                    SavedResult result = await viewModel.SaveFile(tabItem, true);

                    if (result == SavedResult.Cancel) return;
                }

                foreach (Action action in App.ActionsWhenCompleting)
                    action.Invoke();

                _closing = true;

#if DEBUG
                if (_canGetAnException)
                    throw new Exception("Debug exception");
#endif

                Close();
            }
            else
            {
                e.Cancel = false;
            }
        };

        WindowsManager.OtherModalWindows.CollectionChanged += OtherModalWindows_CollectionChanged;

        SetupDragAndDrop();
    }

    public MainWindow(string?[] args) : this()
    {
        MainViewModel? viewModel = this.GetDataContext<MainViewModel>();
        viewModel.GetFilesFromArguments(args);
    }

    private void SetupDragAndDrop()
    {
        void DragLeave(object s, RoutedEventArgs e)
        {
            _isDragging = false;
            DragAndDropWindow? window = WindowsManager.FindModalWindow<DragAndDropWindow>();
            WindowsManager.OtherModalWindows.Remove(window);
            window?.Close();
        }

        void Drop(object sender, DragEventArgs e)
        {
            _isDragging = false;

            if (e.Data.Contains(DataFormats.FileNames))
            {
                IEnumerable<string?> fileNames = e.Data.GetFileNames()!;

                if (fileNames.Any(x => x!.Contains(".dll") || x.Contains(".zip")))
                    this.GetDataContext<MainViewModel>().DropLoadModules(fileNames);
                else
                    this.GetDataContext<MainViewModel>().DropOpenFile(fileNames);
            }

            DragAndDropWindow? window = WindowsManager.FindModalWindow<DragAndDropWindow>();
            WindowsManager.OtherModalWindows.Remove(window);
            window?.Close();
        }

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object s, DragEventArgs e)
    {
        _isDragging = true;

        e.DragEffects &= DragDropEffects.Copy | DragDropEffects.Link;

        DragAndDropWindow window;

        if (e.Data.Contains(DataFormats.FileNames))
        {
            IEnumerable<string>? fileNames = e.Data.GetFileNames();

            if (fileNames.Any(x => x.Contains(".dll") || x.Contains(".zip")))
                window = new DragAndDropWindow(TypeDrop.Module);
            else
                window = new DragAndDropWindow(TypeDrop.File);
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        WindowsManager.OtherModalWindows.Add(window);
        window.Show(this);
    }

    private void OtherModalWindows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDragging) return;

        DragDrop.SetAllowDrop(WindowsManager.MainWindow, e.NewItems is not { Count: > 0 });
    }
}