using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using Regul.Enums;

namespace Regul.Views.Windows;


public class DragAndDropWindow : ContentDialog
{
    private Grid? _moduleTypeGrid;
    private Grid? _fileTypeGrid;

    public DragAndDropWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _moduleTypeGrid = this.FindControl<Grid>("ModuleType");
        _fileTypeGrid = this.FindControl<Grid>("FileType");
    }

    public DragAndDropWindow(TypeDrop typeDrop) : this()
    {
        if (_fileTypeGrid is null || _moduleTypeGrid is null) return;
        
        switch (typeDrop)
        {
            case TypeDrop.Module:
                _fileTypeGrid.IsVisible = false;
                break;
            case TypeDrop.File:
            default:
                _moduleTypeGrid.IsVisible = false;
                break;
        }
    }
}