using System.Globalization;
using Avalonia.Data.Converters;

namespace Regul.Converters;

public class CreateObjectByTypeConverter : IValueConverter
{
    private static readonly Lazy<CreateObjectByTypeConverter> Lazy = new(() => new CreateObjectByTypeConverter());

    public static CreateObjectByTypeConverter Instance
    {
        get => Lazy.Value;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Type type)
            return Activator.CreateInstance(type);
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}