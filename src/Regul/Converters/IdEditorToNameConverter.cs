using System.Globalization;
using Avalonia.Data.Converters;
using Regul.ModuleSystem;

namespace Regul.Converters;

public class IdEditorToNameConverter : IValueConverter
{
    private static readonly Lazy<IdEditorToNameConverter> Lazy = new(() => new IdEditorToNameConverter());

    public static IdEditorToNameConverter Instance
    {
        get => Lazy.Value;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string id) return null;

        string? name = ModuleManager.Editors.FirstOrDefault(x => x.Id == id)?.Name;

        return App.GetString(name ?? "EditorNotDefined");

    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}