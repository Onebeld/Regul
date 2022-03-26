using System;

namespace PleasantUI.Media.Colors
{
    public readonly struct RGB
    {
        public double R { get; }

        public double G { get; }

        public double B { get; }

        public RGB(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        public RGB(RGB rgb)
        {
            R = rgb.R;
            G = rgb.G;
            B = rgb.B;
        }

        public RGB(HSV hsv)
        {
            RGB rgb = hsv.ToRGB();
            R = rgb.R;
            G = rgb.G;
            B = rgb.B;
        }

        public RGB(CMYK cmyk)
        {
            RGB rgb = cmyk.ToRGB();
            R = rgb.R;
            G = rgb.G;
            B = rgb.B;
        }

        public RGB WithR(double r)
        {
            return new RGB(r, G, B);
        }

        public RGB WithG(double g)
        {
            return new RGB(R, g, B);
        }

        public RGB WithB(double b)
        {
            return new RGB(R, G, b);
        }

        public HSV ToHSV()
        {
            return ToHSV(R, G, B);
        }

        public CMYK ToCMYK()
        {
            return ToCMYK(R, G, B);
        }

        public static HSV ToHSV(double r, double g, double b)
        {
            double H = default, S, V;

            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);

            double delta = max - min;

            V = 100.0 * max / 255.0;

            if (max == 0.0) S = 0;
            else S = 100.0 * delta / max;

            if (S == 0)
            {
                H = 0;
            }
            else
            {
                if (r == max) H = 60.0 * (g - b) / delta;
                else if (g == max) H = 120.0 + 60.0 * (b - r) / delta;
                else if (b == max) H = 240.0 + 60.0 * (r - g) / delta;

                if (H < 0.0) H += 360.0;
            }

            return new HSV(H, S, V);
        }

        public static CMYK ToCMYK(double r, double g, double b)
        {
            double division = 1.0 / 255.0;

            double rr = r * division;
            double gg = g * division;
            double bb = b * division;

            double K = 1.0 - Math.Max(Math.Max(rr, gg), bb);
            double C = (1.0 - rr - K) / (1.0 - K);
            double M = (1.0 - gg - K) / (1.0 - K);
            double Y = (1.0 - bb - K) / (1.0 - K);

            C *= 100.0;
            M *= 100.0;
            Y *= 100.0;
            K *= 100.0;

            return new CMYK(C, M, Y, K);
        }
    }
}