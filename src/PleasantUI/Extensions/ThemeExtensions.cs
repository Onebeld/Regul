using System.Text;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Styling;
#if Windows
using Microsoft.Win32;
#endif
using PleasantUI.Media;

namespace PleasantUI.Extensions;

public static class ThemeExtensions
{
    public static string ToAxaml(this Theme theme)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine("<Style xmlns=\"https://github.com/avaloniaui\"");
        stringBuilder.AppendLine("       xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
        stringBuilder.AppendLine("    <Style.Resources>");

        foreach (KeyValuePair<string, uint> color in theme.Colors)
            stringBuilder.AppendLine($"        <Color x:Key=\"{color.Key}\">#{color.Value:X8}</Color>");

        stringBuilder.AppendLine("    </Style.Resources>");
        stringBuilder.AppendLine("</Style>");

        return stringBuilder.ToString();
    }

    public static AvaloniaDictionary<string, uint> ToColorDictionary(this Style styles)
    {
        AvaloniaDictionary<string, uint> colors = new();

        foreach (KeyValuePair<object, object?> keyValuePair in styles.Resources)
            colors.Add((string)keyValuePair.Key, ((Color)keyValuePair.Value!).ToUint32());

        return colors;
    }

    public static string SaveToText(this Theme theme)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine(theme.Name);

        foreach (KeyValuePair<string, uint> color in theme.Colors)
            stringBuilder.AppendLine($"{color.Key};{color.Value}");

        return stringBuilder.ToString();
    }
}