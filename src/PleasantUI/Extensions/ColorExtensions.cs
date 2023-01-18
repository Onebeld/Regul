﻿using Avalonia.Media;
#if Windows
using Avalonia.Win32;
using Microsoft.Win32;
#endif
using PleasantUI.Helpers;

namespace PleasantUI.Extensions;

public static class ColorExtensions
{
    public static Color ChangeColorBrightness(this Color color, double correctionFactor)
    {
        Hsl hsl = ToHsl(color);
        hsl.L = Math.Min(hsl.L / correctionFactor, 1);

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