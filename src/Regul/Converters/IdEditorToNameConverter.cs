using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Regul.ModuleSystem;

namespace Regul.Converters;

public class IdEditorToNameConverter : IValueConverter
{
    private static readonly Lazy<IdEditorToNameConverter> _lazy = new(() => new IdEditorToNameConverter());

    public static IdEditorToNameConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ulong id) return null;

        string? name = ModuleManager.Editors.FirstOrDefault(x => x.Id == id)?.Name;

        return App.GetString(name ?? "EditorNotDefined");

    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}