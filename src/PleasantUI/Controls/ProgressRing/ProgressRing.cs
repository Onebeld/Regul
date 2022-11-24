using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

public partial class ProgressRing : RangeBase
{
    static ProgressRing()
    {
        MinimumProperty.Changed.AddClassHandler<ProgressRing>(OnRangePropertiesChanged);
        ValueProperty.Changed.AddClassHandler<ProgressRing>(OnRangePropertiesChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnRangePropertiesChanged);
    }

    public ProgressRing() => UpdatePseudoClasses(IsIndeterminate, PreserveAspect);


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsIndeterminateProperty)
            UpdatePseudoClasses((bool?)change.NewValue, null);
        else if (change.Property == PreserveAspectProperty)
            UpdatePseudoClasses(null, (bool?)change.NewValue);
    }

    private void UpdatePseudoClasses(
        bool? isIndeterminate,
        bool? preserveAspect)
    {
        if (isIndeterminate.HasValue)
            PseudoClasses.Set(":indeterminate", isIndeterminate.Value);

        if (preserveAspect.HasValue)
            PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
    }

    static void OnRangePropertiesChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        double min = sender.Minimum;
        double ringVal = sender.Value;
        double max = sender.Maximum;

        if (e.NewValue is double newPropVal)
        {
            if (e.Property == MinimumProperty)
                min = newPropVal;
            else if (e.Property == ValueProperty)
                ringVal = newPropVal;
            else if (e.Property == MaximumProperty)
                max = newPropVal;
        }

        sender.ValueAngle = (((ringVal - min) / (max - min)) * 360.0) - 90;
    }
}