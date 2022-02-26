#region

using System;
using System.Globalization;
using Avalonia.Data.Converters;

#endregion

namespace Regul.Base.Converters
{
    public class UShortToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0x" + ((ushort)value).ToString("X4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string str = value.ToString();
                return System.Convert.ToUInt16(str, str.StartsWith("0x") ? 16 : 10);
            }
            catch
            {
                return 0;
            }
        }
    }
}