using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace PleasantUI.Behaviors;

public class SmoothScrollViewerBehavior : Behavior<ScrollViewer>
{
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is null) return;
        
        AssociatedObject.ScrollChanged += AssociatedObjectOnScrollChanged;
    }

    private void AssociatedObjectOnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
    }

    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is null) return;

        AssociatedObject.ScrollChanged -= AssociatedObjectOnScrollChanged;
    }
}