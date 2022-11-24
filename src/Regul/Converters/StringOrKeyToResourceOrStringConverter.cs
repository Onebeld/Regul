using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Regul.Converters;

public class StringOrKeyToResourceOrStringConverter : IValueConverter
{
    private static readonly Lazy<StringOrKeyToResourceOrStringConverter> _lazy = new(() => new StringOrKeyToResourceOrStringConverter());
    public static StringOrKeyToResourceOrStringConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s) return null;

        if (Application.Current is not null && Application.Current.TryFindResource(s, out object? str))
        {
            return str as string;
        }
        
        return s;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}