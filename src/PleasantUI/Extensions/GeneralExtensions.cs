using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace PleasantUI.Extensions;

public static class GeneralExtensions
{
    public static double GetDouble(this string value)
    {
        // Try parsing in the current culture
        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
            // Then try in US english
            !double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
            // Then in neutral language
            !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            result = 1;
        }

        return result;
    }

    public static IBitmap ToBitmap(this WindowIcon icon)
    {
        MemoryStream stream = new();
        icon.Save(stream);
        stream.Position = 0;

        return new Bitmap(stream);
    }
}