using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

/// <summary>
/// Converts the given value into an <see cref="IBrush"/> when a conversion is possible.
/// </summary>
public class ToBrushConverter : IValueConverter
{
    private static readonly Lazy<ToBrushConverter> Lazy = new(() => new ToBrushConverter());

    public static ToBrushConverter Instance
    {
        get => Lazy.Value;
    }

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            IBrush brush => brush,
            Color valueColor => new SolidColorBrush(valueColor),
            HslColor valueHslColor => new SolidColorBrush(valueHslColor.ToRgb()),
            HsvColor valueHsvColor => new SolidColorBrush(valueHsvColor.ToRgb()),
            uint uintColor => new SolidColorBrush(uintColor),
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}