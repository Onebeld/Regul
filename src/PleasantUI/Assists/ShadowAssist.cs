using System;
using System.Globalization;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace PleasantUI.Assists;

public enum ShadowDepth
{
    Depth0,
    Depth1,
    Depth2,
    TabViewDepth,
    TabDepth
}

public static class ShadowProvider
{
    public static BoxShadows ToBoxShadows(this ShadowDepth shadowDepth)
    {
        BoxShadows boxShadows;

        switch (shadowDepth)
        {
            case ShadowDepth.Depth0:
                boxShadows = new BoxShadows();
                break;
            case ShadowDepth.Depth1:
                boxShadows = new BoxShadows(new BoxShadow
                    { Blur = 5, OffsetX = 0, OffsetY = 1, Color = Color.FromArgb(130, 0, 0, 0) });
                break;
            case ShadowDepth.Depth2:
                boxShadows = new BoxShadows(new BoxShadow
                    { Blur = 20, OffsetX = 0, OffsetY = 1, Color = Color.FromArgb(110, 0, 0, 0) });
                break;
            case ShadowDepth.TabViewDepth:
                boxShadows = new BoxShadows(new BoxShadow
                    { Blur = 3, OffsetX = 0, OffsetY = -5, Color = Color.FromArgb(200, 0, 0, 0), Spread = -5, IsInset = true});
                break;
            case ShadowDepth.TabDepth:
                boxShadows = new BoxShadows(new BoxShadow
                    { Blur = 3, OffsetX = 0, OffsetY = 0, Color = Color.FromArgb(200, 0, 0, 0)});
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return boxShadows;
    }
}

public static class ShadowAssist
{
    public static readonly AvaloniaProperty<ShadowDepth> ShadowDepthProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, ShadowDepth>("ShadowDepth", typeof(ShadowAssist));

    static ShadowAssist()
    {
    }

    public static void SetShadowDepth(AvaloniaObject element, ShadowDepth value)
    {
        element.SetValue(ShadowDepthProperty, value);
    }

    public static ShadowDepth GetShadowDepth(AvaloniaObject element)
    {
        return (ShadowDepth)element.GetValue(ShadowDepthProperty);
    }
}