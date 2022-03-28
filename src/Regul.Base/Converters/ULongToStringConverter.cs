using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Regul.Base.Converters;

public class ULongToStringConverter : IValueConverter
{
    public static readonly ULongToStringConverter Instance = new();
        
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return "0x" + ((ulong)value!).ToString("X16");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ulong.TryParse(value?.ToString().Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong result);

        return result;
    }
}