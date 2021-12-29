using Avalonia.Media;
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

namespace PleasantUI.Assists
{
    public enum ShadowDepth
    {
        Depth0,
        Depth1,
        Depth2,
    }

    public static class ShadowProvider
    {
        public static BoxShadows ToBoxShadows(this ShadowDepth shadowDepth)
        {
            BoxShadows boxShadows;

            switch (shadowDepth)
            {
                case ShadowDepth.Depth0:
                    boxShadows = new BoxShadows(new BoxShadow());
                    break;
                case ShadowDepth.Depth1:
                    boxShadows = new BoxShadows(new BoxShadow
                        {Blur = 5, OffsetX = 0, OffsetY = 1, Color = Color.FromArgb(130, 0, 0, 0)});
                    break;
                case ShadowDepth.Depth2:
                    boxShadows = new BoxShadows(new BoxShadow
                        {Blur = 20, OffsetX = 0, OffsetY = 1, Color = Color.FromArgb(110, 0, 0, 0)});
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
            ShadowDepthProperty.Changed.Subscribe(ShadowDepthChangedCallback);
        }

        private static void ShadowDepthChangedCallback(AvaloniaPropertyChangedEventArgs e)
        {
            if (!PleasantSettings.Settings.EnableShadowing) return;

            Border border = e.Sender as Border;
            BoxShadows? boxShadow = border?.BoxShadow;

            if (boxShadow == null) return;

            BoxShadows targetBoxShadows =
                GetShadowDepth((AvaloniaObject) e.Sender).ToBoxShadows();

            if (!border.Classes.Contains("notransitions") && boxShadow.Value.Count > 0)
            {
                Animation animation = new Animation
                    { Duration = TimeSpan.FromMilliseconds(75), FillMode = FillMode.Both };

                animation.Children.Add(
                    new KeyFrame
                    {
                        Cue = Cue.Parse("0%", CultureInfo.CurrentCulture),
                        Setters = {new Setter {Property = Border.BoxShadowProperty, Value = boxShadow}}
                    });
                animation.Children.Add(
                    new KeyFrame
                    {
                        Cue = Cue.Parse("100%", CultureInfo.CurrentCulture),
                        Setters = {new Setter {Property = Border.BoxShadowProperty, Value = targetBoxShadows}}
                    });

                animation.RunAsync(border, null, default);
            }
            else border.SetValue(Border.BoxShadowProperty, targetBoxShadows);
        }

        public static void SetShadowDepth(AvaloniaObject element, ShadowDepth value) =>
            element.SetValue(ShadowDepthProperty, value);

        public static ShadowDepth GetShadowDepth(AvaloniaObject element) =>
            (ShadowDepth) element.GetValue(ShadowDepthProperty);
    }
}