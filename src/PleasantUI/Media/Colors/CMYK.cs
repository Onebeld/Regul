﻿using System;

namespace PleasantUI.Media.Colors;

public readonly struct CMYK
{
    public double C { get; }

    public double M { get; }

    public double Y { get; }

    public double K { get; }

    public CMYK(double c, double m, double y, double k)
    {
        C = c;
        M = m;
        Y = y;
        K = k;
    }

    public CMYK(CMYK cmyk)
    {
        C = cmyk.C;
        M = cmyk.M;
        Y = cmyk.Y;
        K = cmyk.K;
    }

    public CMYK(RGB rgb)
    {
        CMYK cmyk = rgb.ToCMYK();
        C = cmyk.C;
        M = cmyk.M;
        Y = cmyk.Y;
        K = cmyk.K;
    }

    public CMYK(HSV hsv)
    {
        CMYK cmyk = hsv.ToCMYK();
        C = cmyk.C;
        M = cmyk.M;
        Y = cmyk.Y;
        K = cmyk.K;
    }

    public CMYK WithC(double c)
    {
        return new CMYK(c, M, Y, K);
    }

    public CMYK WithM(double m)
    {
        return new CMYK(C, m, Y, K);
    }

    public CMYK WithY(double y)
    {
        return new CMYK(C, M, y, K);
    }

    public CMYK WithK(double k)
    {
        return new CMYK(C, M, Y, k);
    }

    public RGB ToRGB()
    {
        return ToRGB(C, M, Y, K);
    }

    public HSV ToHSV()
    {
        return ToHSV(C, M, Y, K);
    }

    public static RGB ToRGB(double c, double m, double y, double k)
    {
        double R, G, B;

        double cc = c * 0.01;
        double mm = m * 0.01;
        double yy = y * 0.01;
        double kk = k * 0.01;

        R = (1.0 - cc) * (1.0 - kk);
        G = (1.0 - mm) * (1.0 - kk);
        B = (1.0 - yy) * (1.0 - kk);

        R = Math.Round(R * 255.0);
        G = Math.Round(G * 255.0);
        B = Math.Round(B * 255.0);

        return new RGB(R, G, B);
    }

    public static HSV ToHSV(double c, double m, double y, double k)
    {
        return ToRGB(c, m, y, k).ToHSV();
    }
}