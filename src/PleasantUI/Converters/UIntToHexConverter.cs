using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

public class UIntToHexConverter : IValueConverter
{
    private static readonly Lazy<UIntToHexConverter> _lazy = new(() => new UIntToHexConverter());

    public static UIntToHexConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        value is not uint uintColor ? AvaloniaProperty.UnsetValue : $"#{uintColor.ToString("x8").ToUpper()}";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}