using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Regul.Base;
using Regul.S3PE.Interfaces;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Viewers
{
    public class ImageViewer : UserControl, IViewer
    {
        public ImageViewer()
        {
            AvaloniaXamlLoader.Load(this);

            DataContext = new ImageViewerViewModel();
        }

        public ImageViewer(IResource resource, S3PEViewModel mainViewModel) : this()
        {
            Resource = resource;

            ImageViewerViewModel viewModel = this.GetDataContext<ImageViewerViewModel>();

            viewModel.ParentViewModel = mainViewModel;
            viewModel.LoadImage(resource.Stream);
        }

        public IResource Resource { get; set; }
        public Action EditResource { get; set; }

        public void Edit() { }
    }
}