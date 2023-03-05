using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Regul.Converters;

public class KeyToImageConverter : IValueConverter
{
    private static readonly Lazy<KeyToImageConverter> Lazy = new(() => new KeyToImageConverter());
    public static KeyToImageConverter Instance
    {
        get => Lazy.Value;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key) return null;

        if (Application.Current is not null && Application.Current.TryFindResource(key, out object? image))
            return image as DrawingImage;

        return null;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}