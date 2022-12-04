using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Structures;

namespace Regul.Converters;

public class IdEditorToImageConverter : IValueConverter
{
    private static readonly Lazy<IdEditorToImageConverter> Lazy = new(() => new IdEditorToImageConverter());

    public static IdEditorToImageConverter Instance
    {
        get => Lazy.Value;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Editor? editor = ModuleManager.GetEditorById((ulong)value!);
        return editor?.IconKey is null ? App.GetResource<DrawingImage>("UnknownIcon") : App.GetResource<DrawingImage>(editor.IconKey);
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}