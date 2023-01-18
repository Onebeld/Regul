using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Enums;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Enums;
using Regul.Helpers;
using Regul.Managers;
using Regul.Other;
using Regul.Structures;
using Regul.ViewModels;
using Regul.Views.Pages;
using Regul.Views.Windows;

namespace Regul.Views;

public class MainWindow : PleasantWindow
{
    private DragAndDropWindow? _dragAndDropWindow;

#if DEBUG
    private bool _canGetAnException;
#endif

    private bool _closing;
    private WindowNotificationManager _notificationManager;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDevTools();
#endif

#if DEBUG
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.LeftShift)
                _canGetAnException = true;
        };

        KeyUp += (_, e) =>
        {
            if (e.Key == Key.LeftShift)
                _canGetAnException = false;
        };
#endif

        MainWindowViewModel viewModel = new();

        DataContext = viewModel;
        ViewModel = viewModel;

        SetupDragAndDrop();

        Closing += OnClosing;
        TemplateApplied += (_, _) =>
        {
            _notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3,
                ZIndex = 1
            };
        };
        Loaded += (_, _) =>
        {
            List<string> filesFromArguments = ViewModel.GetFilesFromArguments(Program.Arguments);
            ViewModel.DropFiles(filesFromArguments);

            ViewModel.SynchronizationContext = SynchronizationContext.Current;

            if (!Directory.Exists(RegulDirectories.Cache))
                Directory.CreateDirectory(RegulDirectories.Cache);
            if (!Directory.Exists(Path.Combine(RegulDirectories.Cache, "OpenFiles")))
                Directory.CreateDirectory(Path.Combine(RegulDirectories.Cache, "OpenFiles"));

            foreach (string file in Directory.EnumerateFiles(Path.Combine(RegulDirectories.Cache, "OpenFiles")))
            {
                string content = File.ReadAllText(file);

                File.Delete(file);
                List<string> files = ViewModel.GetFilesFromArguments(content.Split('|'));
                ViewModel.DropFiles(files);
            }

            if (ViewModel.SynchronizationContext is not null)
                Task.Run(() => ViewModel.LaunchEventWaitHandler(ViewModel.SynchronizationContext));
            
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        };
    }
    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;

        if (!_closing)
        {
            foreach (Workbench workbench in ViewModel.Workbenches)
            {
                if (!workbench.IsDirty) continue;
                
                if (ViewModel.Content is not EditorsPage)
                    WindowsManager.MainWindow?.ChangePage(typeof(EditorsPage), TitleBarType.ExtendedWithoutContent);

                ViewModel.SelectedWorkbench = workbench;
                
                SaveResult saveResult = await SaveBeforeClosingWorkbench(workbench);
                
                if (saveResult == SaveResult.Cancel)
                    return;
            }

            ApplicationSettings.Save();
            PleasantUiSettings.Save();

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
    }

    public async Task<SaveResult> SaveBeforeClosingWorkbench(Workbench? workbench)
    {
        string result = await MessageBox.Show(
            this, 
            $"{App.GetString("YouWantToSaveProject")}: {Path.GetFileName(workbench.PathToFile) ?? App.GetString("NoName")}?", 
            string.Empty,
            MessageBoxButtons.YesNoCancel);

        if (result == "Yes")
        {
            SaveResult saveResult = await ViewModel.SaveWorkbench(workbench);

            if (saveResult == SaveResult.Success)
                return SaveResult.Success;
        }
        if (result == "No")
            return SaveResult.Success;

        return SaveResult.Cancel;
    }

    private void SetupDragAndDrop()
    {
        void DragLeave(object? s, DragEventArgs dragEventArgs)
        {
            _dragAndDropWindow?.Close();
            _dragAndDropWindow = null;
        }

        void DragEnter(object? s, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Copy | DragDropEffects.Link;

            if (e.Data.Contains(DataFormats.FileNames))
            {
                IEnumerable<string>? fileNames = e.Data.GetFileNames();

                if (fileNames is null) return;

                if (fileNames.Any(x => x.Contains(".dll") || x.Contains(".zip")))
                    _dragAndDropWindow = new DragAndDropWindow(TypeDrop.Module);
                else
                    _dragAndDropWindow = new DragAndDropWindow(TypeDrop.File);
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            _dragAndDropWindow.Show(this);
        }

        void Drop(object? s, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                IEnumerable<string>? fileNames = e.Data.GetFileNames();

                if (fileNames is not null)
                {
                    List<string> fileNamesList = new(fileNames);

                    if (fileNamesList.Any(x => x.Contains(".dll") || x.Contains(".zip")))
                        ViewModel.DropModules(fileNamesList);
                    else
                        ViewModel.DropFiles(fileNamesList);
                }
            }

            _dragAndDropWindow?.Close();
            _dragAndDropWindow = null;
        }

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    public void ShowNotification(string text, NotificationType type = NotificationType.Information, TimeSpan? timeSpan = null)
    {
        string titleValue = type switch
        {

            NotificationType.Information => App.GetString("Information"),
            NotificationType.Success => App.GetString("Successful"),
            NotificationType.Warning => App.GetString("Warning"),
            NotificationType.Error => App.GetString("Error"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        string textValue = App.GetString(text);

        _notificationManager.Show(new Notification(titleValue, textValue, type, timeSpan));
    }

    public void ChangePage(Type? pageType, TitleBarType titleBarType = TitleBarType.Classic)
    {
        if (pageType is not null)
            ChangePage(Activator.CreateInstance(pageType), titleBarType);
    }

    public void ChangePage(object? page, TitleBarType titleBarType = TitleBarType.Classic)
    {
        MainWindowViewModel viewModel = this.GetDataContext<MainWindowViewModel>();
        viewModel.Content = page;

#if !OSX
        TitleBarType = titleBarType;
#endif
    }

    public void RunLoading(double maximum)
    {
        ViewModel.LoadingWindow = new LoadingWindow
        {
            Maximum = maximum
        };
        ViewModel.LoadingWindow.Show(this);
    }

    public bool LoadingIsOpened() => ViewModel.LoadingWindow is not null && !ViewModel.LoadingWindow.IsClosed;

    public void ChangeLoadingProgress(double progress, string userState, bool isIndeterminate)
    {
        if (ViewModel.LoadingWindow is null)
            throw new NullReferenceException("LoadingWindow is null");
        
        ViewModel.SynchronizationContext?.Send(_ =>
        {
            ViewModel.LoadingWindow.IsIndeterminate = isIndeterminate;
            ViewModel.LoadingWindow.Value = progress;
            ViewModel.LoadingWindow.Text = App.GetString(userState);
        }, "");
    }

    public void CloseLoading() => ViewModel.LoadingWindow?.Close();
}