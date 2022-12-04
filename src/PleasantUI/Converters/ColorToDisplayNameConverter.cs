using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Helpers;

namespace PleasantUI.Converters;

/// <summary>
/// Gets the approximated display name for the color.
/// </summary>
public class ColorToDisplayNameConverter : IValueConverter
{
    private static readonly Lazy<ColorToDisplayNameConverter> Lazy = new(() => new ColorToDisplayNameConverter());

    public static ColorToDisplayNameConverter Instance
    {
        get => Lazy.Value;
    }

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color color;

        switch (value)
        {
            case Color valueColor:
                color = valueColor;
                break;
            case HslColor valueHslColor:
                color = valueHslColor.ToRgb();
                break;
            case HsvColor valueHsvColor:
                color = valueHsvColor.ToRgb();
                break;
            case SolidColorBrush valueBrush:
                color = valueBrush.Color;
                break;
            default:
                // Invalid color value provided
                return AvaloniaProperty.UnsetValue;
        }

        // ColorHelper.ToDisplayName ignores the alpha component
        // This means fully transparent colors will be named as a real color
        // That undesirable behavior is specially overridden here
        return color.A == 0x00 ? AvaloniaProperty.UnsetValue : ColorHelper.ToDisplayName(color);
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}