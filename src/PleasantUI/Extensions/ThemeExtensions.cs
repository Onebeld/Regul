using System.Text.Json;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Media;
using PleasantUI.Other;

namespace PleasantUI.Extensions;

public static class ThemeExtensions
{
    public static IResourceDictionary ToResourceDictionary(this Theme theme)
    {
        ResourceDictionary resourceDictionary = new();

        foreach (KeyColor color in theme.Colors)
            resourceDictionary.Add(color.Key, Color.FromUInt32(color.Value));
        
        return resourceDictionary;
    }

    public static AvaloniaList<KeyColor> ToColorList(this IResourceDictionary resourceDictionary)
    {
        AvaloniaList<KeyColor> colors = new();

        foreach (KeyValuePair<object, object?> keyValuePair in resourceDictionary)
            colors.Add(new KeyColor((string)keyValuePair.Key, ((Color)keyValuePair.Value!).ToUint32()));

        return colors;
    }

    public static void SaveToJson(this Theme theme, Stream stream) => JsonSerializer.Serialize(stream, theme);
    public static string SaveToJson(this Theme theme) => JsonSerializer.Serialize(theme);
}