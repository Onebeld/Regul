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
#if Windows
#pragma warning disable CA1416
    public static bool WindowsThemeIsLight()
    {
        using RegistryKey? registry = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

        try
        {
            string? appsUseLightTheme = registry?.GetValue("AppsUseLightTheme")?.ToString();
            if (appsUseLightTheme is not null && int.TryParse(appsUseLightTheme, out int val))
                return val == 1;
        }
        catch { }

        try
        {
            string? systemUsesLightTheme = registry?.GetValue("SystemUsesLightTheme")?.ToString();
            if (systemUsesLightTheme is not null && int.TryParse(systemUsesLightTheme, out int val2))
                return val2 == 1;
        }
        catch { }


        return true;
    }
#pragma warning disable CA1416
#endif

    public static string ToAxaml(this Theme theme)
    {
        StringBuilder stringBuilder = new();
        
        stringBuilder.AppendLine("<Style xmlns=\"https://github.com/avaloniaui\"");
        stringBuilder.AppendLine("       xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
        stringBuilder.AppendLine("    <Style.Resources>");

        foreach (KeyValuePair<string,uint> color in theme.Colors)
            stringBuilder.AppendLine($"        <Color x:Key=\"{color.Key}\">#{color.Value:X8}</Color>");

        stringBuilder.AppendLine("    </Style.Resources>");
        stringBuilder.AppendLine("</Style>");

        return stringBuilder.ToString();
    }

    public static AvaloniaDictionary<string, uint> ToColorDictionary(this Style styles)
    {
        AvaloniaDictionary<string, uint> colors = new();

        foreach (KeyValuePair<object,object?> keyValuePair in styles.Resources)
            colors.Add((string)keyValuePair.Key, ((Color)keyValuePair.Value!).ToUint32());

        return colors;
    }

    public static string SaveToText(this Theme theme)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine(theme.Name);

        foreach (KeyValuePair<string,uint> color in theme.Colors)
            stringBuilder.AppendLine($"{color.Key};{color.Value}");

        return stringBuilder.ToString();
    }
}