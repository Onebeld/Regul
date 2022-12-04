using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using Regul.ViewModels.Windows;

namespace Regul.Views.Windows;

public class OpenProjectWindow : ContentDialog
{
    public OpenProjectViewModel ViewModel { get; }

    public OpenProjectWindow()
    {
        AvaloniaXamlLoader.Load(this);

        OpenProjectViewModel viewModel = new(this);

        ViewModel = viewModel;
        DataContext = viewModel;
    }
}