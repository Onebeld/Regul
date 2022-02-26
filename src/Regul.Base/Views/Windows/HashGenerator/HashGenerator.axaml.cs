#region

using Avalonia;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

#endregion

namespace Regul.Base.Views.Windows
{
    public class HashGenerator : PleasantWindow
    {
        public HashGenerator()
        {
            AvaloniaXamlLoader.Load(this);

            this.AttachDevTools();

            Closed += (s, e) => { this.GetDataContext<HashGeneratorViewModel>().Close(this); };
        }
    }
}