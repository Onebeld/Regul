using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

public class ColorToHexConverter : IValueConverter
{
    private static readonly Lazy<ColorToHexConverter> _lazy = new(() => new ColorToHexConverter());

    public static ColorToHexConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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

        uint hexColor = color.ToUint32();
        
        return hexColor.ToString("x8").ToUpper();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string hexValue = value?.ToString() ?? string.Empty;

        if (Color.TryParse(hexValue, out Color color))
        {
            return color;
        }

        if (hexValue.StartsWith("#", StringComparison.Ordinal) == false &&
            Color.TryParse("#" + hexValue, out Color color2))
        {
            return color2;
        }

        // Invalid hex color value provided
        return AvaloniaProperty.UnsetValue;
    }
}