using Avalonia;
using Avalonia.Controls;
using Path = Avalonia.Controls.Shapes.Path;

namespace PleasantUI.Controls;

public partial class PleasantTitleBar
{
    private PleasantCaptionButtons? _captionButtons;

    private MenuItem? _closeMenuItem;
    private MenuItem? _collapseMenuItem;
    private MenuItem? _expandMenuItem;
    private MenuItem? _reestablishMenuItem;
    private Separator? _separator;

    private Border? _dragWindowBorder;
    private Image? _image;
    private Path? _logoPath;

    private TextBlock? _title;
    private StackPanel? _titlePanel;
    private TextBlock? _description;

    private PleasantWindow? _host;

    public static readonly StyledProperty<bool> IsMacOsProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(IsMacOs));

    public bool IsMacOs
    {
        get => GetValue(IsMacOsProperty);
        set => SetValue(IsMacOsProperty, value);
    }
}