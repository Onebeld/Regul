using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;

namespace Regul.Views.Windows;

public class LoadingWindow : ContentDialog
{
    private readonly TextBlock? _textBlock;
    private readonly ProgressBar? _progressBar;

    public string? Text
    {
        get => _textBlock?.Text;
        set
        {
            if (_textBlock is not null)
                _textBlock.Text = value;
        }
    }

    public bool IsIndeterminate
    {
        get => _progressBar is { IsIndeterminate: true };
        set
        {
            if (_progressBar is not null)
                _progressBar.IsIndeterminate = value;
        }
    }

    public double Value
    {
        get => _progressBar?.Value ?? double.NaN;
        set
        {
            if (_progressBar is not null)
                _progressBar.Value = value;
        }
    }
    
    public double Maximum
    {
        get => _progressBar?.Maximum ?? double.NaN;
        set
        {
            if (_progressBar is not null)
                _progressBar.Maximum = value;
        }
    }

    public LoadingWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _textBlock = this.FindControl<TextBlock>("TextBlock");
        _progressBar = this.FindControl<ProgressBar>("ProgressBar");
    }
}