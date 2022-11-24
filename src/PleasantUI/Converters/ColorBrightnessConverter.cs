using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Extensions;

namespace PleasantUI.Converters;

public class ColorBrightnessConverter : IValueConverter
{
    private static readonly Lazy<ColorBrightnessConverter> _lazy = new(() => new ColorBrightnessConverter());
    
    public static ColorBrightnessConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color || parameter is not string s || !double.TryParse(s, out double res))
            return AvaloniaProperty.UnsetValue;

        return color.ChangeColorBrightness(res);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}