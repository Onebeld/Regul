using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using PleasantUI.Xaml.Interactivity;

namespace PleasantUI.Xaml.Behaviors;

// This Behavior fixes the Flyout opening animation.
public class OnCloseFlyoutOpacityBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        if (AssociatedObject is null) return;
        
        AssociatedObject.DetachedFromLogicalTree += AssociatedObjectOnDetachedFromLogicalTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is null) return;

        AssociatedObject.DetachedFromLogicalTree -= AssociatedObjectOnDetachedFromLogicalTree;
        AssociatedObject.DetachedFromVisualTree -= AssociatedObjectOnDetachedFromVisualTree;
    }
    
    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Border border)
            border.Opacity = 0;
    }
    
    private void AssociatedObjectOnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (sender is Border border)
            border.Opacity = 0;
    }
}