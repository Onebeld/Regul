using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Regul.Base;
using Regul.S3PE.Interfaces;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Viewers
{
    public class DDSViewer : UserControl, IViewer
    {
        public DDSViewer()
        {
            AvaloniaXamlLoader.Load(this);

            DataContext = new DDSViewerViewModel();
        }
        public DDSViewer(IResource resource) : this()
        {
            Resource = resource;

            this.GetDataContext<DDSViewerViewModel>().LoadDDS(resource.Stream);
        }

        public IResource Resource { get; set; }
        public Action EditResource { get; set; }

        public void Edit() { }
    }
}