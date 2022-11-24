using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls;

public partial class ProgressRing
{
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>();

    public static readonly StyledProperty<bool> PreserveAspectProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(PreserveAspect), true);

    public static readonly StyledProperty<double> ValueAngleProperty = 
        AvaloniaProperty.Register<ProgressRing, double>(nameof(ValueAngle), -90.0);

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public bool PreserveAspect
    {
        get => GetValue(PreserveAspectProperty);
        set => SetValue(PreserveAspectProperty, value);
    }

    public double ValueAngle
    {
        get => GetValue(ValueAngleProperty);
        private set => SetValue(ValueAngleProperty, value);
    }
}