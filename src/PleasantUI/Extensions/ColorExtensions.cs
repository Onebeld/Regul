using Avalonia.Media;
#if Windows
using Avalonia.Win32;
using Microsoft.Win32;
#endif
using PleasantUI.Helpers;

namespace PleasantUI.Extensions;

public static class ColorExtensions
{
#if Windows
#pragma warning disable CA1416
    public static uint GetWindowsAccentColor()
    {
        if (Win32Platform.WindowsVersion < new Version(10, 0, 10586))
            return PleasantUiSettings.DefaultAccentColor;

        const string dwmKey = @"Software\Microsoft\Windows\DWM";

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(dwmKey);

        if (key is null)
            throw new InvalidOperationException("The \"HKCU\\" + dwmKey + "\" registry key does not exist.");

        object? accentColorObject = key.GetValue("AccentColor");
        if (accentColorObject is int accentColorDword)
        {
            uint a = (uint)((accentColorDword >> 24) & 0xFF);
            uint b = (uint)((accentColorDword >> 16) & 0xFF);
            uint g = (uint)((accentColorDword >> 8) & 0xFF);
            uint r = (uint)((accentColorDword >> 0) & 0xFF);

            return (a << 24) | (r << 16) | (g << 8) | b << 0;
        }

        throw new InvalidOperationException("The \"HKCU\\" + dwmKey + "\\AccentColor\" registry key value could not be parsed as an ABGR color.");
    }
#pragma warning disable CA1416
#endif

    public static Color ChangeColorBrightness(this Color color, double coefficient)
    {
        Hsl hsl = ToHsl(color);
        hsl.L = Math.Min(hsl.L / coefficient, 1);

        return hsl.ToRgb(color.A);
    }

    private static Hsl ToHsl(this Color color)
    {
        const double div = 1.0 / 255;

        double rr = color.R * div;
        double gg = color.G * div;
        double bb = color.B * div;

        double min = Math.Min(Math.Min(rr, gg), bb);
        double max = Math.Max(Math.Max(rr, gg), bb);
        double delta = max - min;

        double h = 0;
        double s = 0;
        double l = (max + min) * 0.5;

        if (delta != 0)
        {
            s = l <= 0.5 ? delta / (max + min) : delta / (2 - max - min);

            if (rr == max)
                h = (gg - bb) / 6 / delta;
            else if (gg == max)
                h = 1.0 / 3 + (bb - rr) / 6 / delta;
            else if (bb == max)
                h = 2.0 / 3 + (rr - gg) / 6 / delta;
        }

        if (h < 0) h += 1;
        if (h > 1) h -= 1;

        return new Hsl(h * 360, s, l);
    }
}