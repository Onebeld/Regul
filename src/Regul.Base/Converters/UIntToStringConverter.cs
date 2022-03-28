using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Regul.Base.Converters;

public class UIntToStringConverter : IValueConverter
{
    public static readonly UIntToStringConverter Instance = new();
        
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return "0x" + ((uint)value!).ToString("X8");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        uint.TryParse(value?.ToString().Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out uint result);

        return result;
    }
}