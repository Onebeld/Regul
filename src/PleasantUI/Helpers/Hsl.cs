using Avalonia.Media;

namespace PleasantUI.Helpers;

public struct Hsl
{
    public double H;
    public double S;
    public double L;

    public Hsl(double hue, double saturation, double luminosity)
    {
        H = hue;
        S = saturation;
        L = luminosity;
    }

    public Color ToRgb(byte alpha = 255)
    {
        double r, g, b;

        if (S == 0)
            r = g = b = L * 255;
        else
        {
            const double div1 = 1.0 / 360;
            double newHue = (float)H * div1;

            double v2 = L < 0.5
                ? L * (1 + S)
                : L + S - L * S;
            double v1 = 2 * L - v2;

            const double div = 1.0 / 3;

            r = 255 * HueToRgb(v1, v2, newHue + div);
            g = 255 * HueToRgb(v1, v2, newHue);
            b = 255 * HueToRgb(v1, v2, newHue - div);
        }

        return Color.FromArgb(alpha, (byte)r, (byte)g, (byte)b);
    }

    private static double HueToRgb(double v1, double v2, double hue)
    {
        if (hue < 0)
            hue += 1;

        if (hue > 1)
            hue -= 1;

        if (6 * hue < 1)
            return v1 + (v2 - v1) * 6 * hue;

        if (2 * hue < 1)
            return v2;

        if (3 * hue < 2)
            return v1 + (v2 - v1) * (2.0f / 3 - hue) * 6;

        return v1;
    }
}