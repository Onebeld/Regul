using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Regul.Base.Converters
{
    public class UShortToStringConverter : IValueConverter
    {
        public static readonly UShortToStringConverter Instance = new UShortToStringConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0x" + ((ushort)value).ToString("X4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ushort.TryParse(value.ToString().Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ushort result);

            return result;
        }
    }
}