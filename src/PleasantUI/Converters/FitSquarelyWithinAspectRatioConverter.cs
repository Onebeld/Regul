using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

public class FitSquarelyWithinAspectRatioConverter : IValueConverter
{
    private static readonly Lazy<FitSquarelyWithinAspectRatioConverter> Lazy = new(() => new FitSquarelyWithinAspectRatioConverter());

    public static FitSquarelyWithinAspectRatioConverter Instance
    {
        get => Lazy.Value;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Rect bounds = (Rect)value!;
        return Math.Min(bounds.Width, bounds.Height);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}