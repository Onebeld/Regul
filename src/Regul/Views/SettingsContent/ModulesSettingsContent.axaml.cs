using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Regul.Views.SettingsContent;

public class ModulesSettingsContent : UserControl
{
    public ModulesSettingsContent()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}