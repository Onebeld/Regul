using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Assist;
using PleasantUI.Xaml.Interactivity;

namespace PleasantUI.Xaml.Behaviors;

public class ShadowBehavior : Behavior<Control>
{
    public static readonly StyledProperty<ShadowDepth> ShadowDepthProperty =
        AvaloniaProperty.Register<ShadowBehavior, ShadowDepth>(nameof(ShadowDepth));

    public static readonly StyledProperty<bool> EnableShadowingProperty =
        AvaloniaProperty.Register<ShadowBehavior, bool>(nameof(EnableShadowing));

    public ShadowDepth ShadowDepth
    {
        get => GetValue(ShadowDepthProperty);
        set => SetValue(ShadowDepthProperty, value);
    }

    public bool EnableShadowing
    {
        get => GetValue(EnableShadowingProperty);
        set => SetValue(EnableShadowingProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        if (EnableShadowing)
            ApplyShadow();
        else AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (AssociatedObject is null) return;

        if (EnableShadowing)
            ApplyShadow();
        else
            AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    private void ApplyShadow()
    {
        if (AssociatedObject is null) return;

        BoxShadows targetBoxShadows = ShadowDepth.ToBoxShadows();
        AssociatedObject.SetValue(Border.BoxShadowProperty, targetBoxShadows);
    }
}