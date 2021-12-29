using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Regul.S3PE.Interfaces;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Viewers
{
    public class ExceptionViewer : UserControl, IViewer
    {
        public TextBox TextBox { get; set; }

        public ExceptionViewer()
        {
            AvaloniaXamlLoader.Load(this);
            
            TextBox = this.FindControl<TextBox>("PART_TextBox");
        }
        public ExceptionViewer(Exception ex) : this()
        {
            TextBox.Text = ex.ToString();
        }

        public IResource Resource { get; set; }
        public Action EditResource { get; set; }

        public void Edit() { }
    }
}