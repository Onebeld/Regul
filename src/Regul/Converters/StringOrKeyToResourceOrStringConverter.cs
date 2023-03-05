using System.Globalization;
using Avalonia.Data.Converters;

namespace Regul.Converters;

public class StringOrKeyToResourceOrStringConverter : IValueConverter
{
    private static readonly Lazy<StringOrKeyToResourceOrStringConverter> Lazy = new(() => new StringOrKeyToResourceOrStringConverter());
    public static StringOrKeyToResourceOrStringConverter Instance
    {
        get => Lazy.Value;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string s ? null : App.GetString(s);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}