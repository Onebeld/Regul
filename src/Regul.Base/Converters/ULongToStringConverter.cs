#region

using System;
using System.Globalization;
using Avalonia.Data.Converters;

#endregion

namespace Regul.Base.Converters
{
    public class ULongToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0x" + ((ulong)value).ToString("X16");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string str = value.ToString();
                return System.Convert.ToUInt64(str, str.StartsWith("0x") ? 16 : 10);
            }
            catch
            {
                return 0;
            }
        }
    }
}