using Avalonia.Controls;
using Avalonia.Metadata;

namespace PleasantUI.Xaml.Behaviors;

public class SharedContent : Control
{
    [Content]
    public object? Content { get; set; }
}