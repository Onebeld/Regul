using System.Globalization;
using Avalonia.Data.Converters;

namespace Regul.Converters;

public class PathToFileNameConverter : IValueConverter
{
    private static readonly Lazy<PathToFileNameConverter> Lazy = new(() => new PathToFileNameConverter());
    public static PathToFileNameConverter Instance
    {
        get => Lazy.Value;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? path = value as string;

        return !string.IsNullOrWhiteSpace(path) ? Path.GetFileName(path) : App.GetString("NoName");
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}