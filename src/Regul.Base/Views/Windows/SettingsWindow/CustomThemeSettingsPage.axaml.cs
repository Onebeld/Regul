#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

#endregion

namespace Regul.Base.Views.Windows
{
    public class CustomThemeSettingsPage : UserControl
    {
        public CustomThemeSettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}