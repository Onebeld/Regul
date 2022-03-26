using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;

namespace Regul.Base.Views.Windows
{
    public class HashGenerator : PleasantWindow
    {
        public HashGenerator()
        {
            AvaloniaXamlLoader.Load(this);

            Closed += (s, e) => { this.GetDataContext<HashGeneratorViewModel>().Close(this); };
        }
    }
}