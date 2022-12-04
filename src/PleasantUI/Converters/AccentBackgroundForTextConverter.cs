using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PleasantUI.Helpers;

namespace PleasantUI.Converters;

public class AccentBackgroundForTextConverter : IValueConverter
{
    private static readonly Lazy<AccentBackgroundForTextConverter> Lazy = new(() => new AccentBackgroundForTextConverter());
    public static AccentBackgroundForTextConverter Instance
    {
        get => Lazy.Value;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color)
            return Colors.White;
        double lum = ColorHelper.GetRelativeLuminance(color);

        Color foregroundColor = lum <= 0.2 ? Colors.White : Colors.Black;
        return foregroundColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}