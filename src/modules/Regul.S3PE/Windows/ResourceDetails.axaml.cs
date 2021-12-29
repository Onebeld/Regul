using System.Linq;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.Base;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Windows
{
    public class ResourceDetails : PleasantDialogWindow
    {
        public ResourceDetails() : this(true) { }
        public ResourceDetails(bool useName = true)
        {
            AvaloniaXamlLoader.Load(this);

            DataContext = new ResourceDetailsViewModel(useName);
            
            SetupDragAndDrop();
        }
        public ResourceDetails(IResourceKey key, bool useName = true)
        {
            AvaloniaXamlLoader.Load(this);

            DataContext = new ResourceDetailsViewModel(useName, key);
            
            SetupDragAndDrop();
        }

        private void SetupDragAndDrop()
        {
            void DragOver(object s, DragEventArgs e)
            {
                e.DragEffects &= DragDropEffects.Copy | DragDropEffects.Link;

                if (!e.Data.Contains(DataFormats.FileNames)) e.DragEffects = DragDropEffects.None;
            }

            void Drop(object s, DragEventArgs e)
            {
                if (e.Data.Contains(DataFormats.FileNames))
                {
                    this.GetDataContext<ResourceDetailsViewModel>().FileName = e.Data.GetFileNames()?.First();
                }
            }
            
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }
    }
}