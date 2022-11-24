using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

public class FitSquarelyWithinAspectRatioConverter : IValueConverter
{
    private static readonly Lazy<FitSquarelyWithinAspectRatioConverter> _lazy = new(() => new FitSquarelyWithinAspectRatioConverter());

    public static FitSquarelyWithinAspectRatioConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Rect bounds = (Rect)value!;
        return Math.Min(bounds.Width, bounds.Height);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}