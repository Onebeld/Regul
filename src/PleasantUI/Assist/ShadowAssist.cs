using Avalonia.Media;

namespace PleasantUI.Assist;

public enum ShadowDepth
{
    SmallWindow,
    ModalWindowDepth,
    ColorPreviewer
}

public static class ShadowProvider
{
    public static BoxShadows ToBoxShadows(this ShadowDepth shadowDepth)
    {
        BoxShadows boxShadows = shadowDepth switch
        {
            ShadowDepth.SmallWindow => new BoxShadows(new BoxShadow
            {
                Blur = 10, OffsetX = 0, OffsetY = 1, Color = Color.FromArgb(60, 0, 0, 0)
            }),
            ShadowDepth.ModalWindowDepth => new BoxShadows(new BoxShadow
            {
                Blur = 60, OffsetX = 0, OffsetY = 6, Color = Color.FromArgb(110, 0, 0, 0)
            }),
            ShadowDepth.ColorPreviewer => new BoxShadows(new BoxShadow
            {
                Blur = 10, OffsetX = 0, OffsetY = 0, Color = Color.FromArgb(190, 0, 0, 0)
            }),
            _ => throw new ArgumentOutOfRangeException()
        };

        return boxShadows;
    }
}