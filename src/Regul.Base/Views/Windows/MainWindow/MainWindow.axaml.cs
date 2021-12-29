using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PleasantUI;
using PleasantUI.Controls.Custom;
using Regul.Base.DragAndDrop;
using Regul.Base.Other;
using Regul.ModuleSystem;

namespace Regul.Base.Views.Windows
{
    public class MainWindow : PleasantWindow
    {
        private bool _closing;

#if DEBUG
        private bool _canGetAnException;
#endif


		public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);

            this.AttachDevTools();

            MainViewModel viewModel = new MainViewModel();

            DataContext = viewModel;
            
            viewModel.NotificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3,
                ZIndex = 1
            };

#if DEBUG
            KeyDown += (s,e) =>
			{
                if (e.Key == Key.LeftShift)
                    _canGetAnException = true;
                else _canGetAnException = false;
			};
#endif

			TemplateApplied += (s, e) => this.GetDataContext<MainViewModel>().Initialize();
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

                    for (int i1 = 0; i1 < ModuleManager.System.Modules.Count; i1++)
                        ModuleManager.System.Modules[i1].Release();

                    _closing = true;

#if DEBUG
                    if (_canGetAnException)
                        throw new System.Exception("Debug exception");
#endif

					Close();
                }
                else e.Cancel = false;
            };

			WindowsManager.OtherModalWindows.CollectionChanged += OtherModalWindows_CollectionChanged;
        }

        private void SetupDragAndDrop()
		{
            void DragLeave(object s, RoutedEventArgs e)
			{
                DragAndDropWindow window = WindowsManager.FindModalWindow<DragAndDropWindow>();
                WindowsManager.OtherModalWindows.Remove(window);
                window?.Close();
			}

            AddHandler(DragDrop.DragOverEvent, DragOver);
            AddHandler(DragDrop.DragLeaveEvent, DragLeave);
		}
        
        private void DragOver(object s, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Copy | DragDropEffects.Link;

            if (!e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            DragAndDropWindow window = null;

            if (e.Data.Contains(".dll"))
            {
                window = new DragAndDropWindow(TypeDrop.OnlyModule);
            }
            else if (e.Data.Contains(".*"))
            {
                window = new DragAndDropWindow(TypeDrop.OnlyFile);
            }

            if (window != null)
            {
                WindowsManager.OtherModalWindows.Add(window);
                window.Show(this);
            }
        }

		private void OtherModalWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null && e.NewItems.Count > 0)
                DragDrop.SetAllowDrop(WindowsManager.MainWindow, false);
            else DragDrop.SetAllowDrop(WindowsManager.MainWindow, true);
		}
	}
}