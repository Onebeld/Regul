using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using PleasantUI.Assists;

namespace PleasantUI.Behaviors;

public class ShadowBehavior : Behavior<Control>
{
    public static readonly StyledProperty<ShadowDepth> ShadowDepthProperty =
        AvaloniaProperty.Register<ShadowBehavior, ShadowDepth>(nameof(ShadowDepth), ShadowDepth.Depth1);

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
        if (EnableShadowing && ShadowDepth != ShadowDepth.Depth0)
            ApplyShadow();
        else AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.SetValue(Border.BoxShadowProperty, default);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);
        
        if (AssociatedObject is null) return;

        if (EnableShadowing && ShadowDepth != ShadowDepth.Depth0)
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