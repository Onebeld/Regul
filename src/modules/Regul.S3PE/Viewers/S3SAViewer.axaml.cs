using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Regul.Base;
using Regul.Base.Views.Windows;
using Regul.S3PE.Interfaces;
using Regul.S3PI.Interfaces;
using Regul.S3PI.Resources;

namespace Regul.S3PE.Viewers
{
    public class S3SAViewer : UserControl, IViewer
    {
        public AvaloniaEdit.TextEditor AvaloniaEdit { get; set; }

        public Button Import { get; set; }
        public Button Export { get; set; }

        public S3SAViewer()
        {
            AvaloniaXamlLoader.Load(this);
            
            AvaloniaEdit = this.FindControl<AvaloniaEdit.TextEditor>("PART_AvaloniaEdit");
            Import = this.FindControl<Button>("PART_Import");
            Export = this.FindControl<Button>("PART_Export");
        }
        public S3SAViewer(IResource resource, Action importDll) : this()
        {
            Resource = resource;
            ScriptResource scriptResource = Resource as ScriptResource;
            AvaloniaEdit.Document.Text = scriptResource.Value;
            
            Export.Click += ExportOnClick;
            Import.Click += (s, e) => importDll?.Invoke();
        }

        private async void ExportOnClick(object sender, RoutedEventArgs e)
        {
            ScriptResource scriptResource = Resource as ScriptResource;

            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialFileName = scriptResource.GetAssemblyName(),
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = $"DLL-{App.GetResource<string>("Files")}", Extensions = {"dll"}
                    }
                }
            };
            
            string s = await dialog.ShowAsync(WindowsManager.MainWindow);

            if (!string.IsNullOrEmpty(s))
            {
                byte[] data = new byte[scriptResource.Assembly.BaseStream.Length];
                await scriptResource.Assembly.BaseStream.ReadAsync(data, 0, data.Length);

                using (FileStream fileStream = new FileStream(s, FileMode.Create, FileAccess.Write))
                    await fileStream.WriteAsync(data, 0, data.Length);
                
                WindowsManager.MainWindow.GetDataContext<MainViewModel>().NotificationManager.Show(
                    new Notification(App.GetResource<string>("Successful"), App.GetResource<string>("FileExported"), NotificationType.Success));
            }
        }

        public IResource Resource { get; set; }
        public Action EditResource { get; set; }

        public void Edit() { }
    }
}