using Avalonia.Controls.Converters;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

public static class GlobalConverters
{
    public static readonly IValueConverter NameToFontFamily =
        new FuncValueConverter<string, FontFamily>(FontFamily.Parse!);

    public static readonly IValueConverter DoubleInverse =
        new FuncValueConverter<double, double>(value => -value);

    public static readonly IValueConverter UIntToBrush =
        new FuncValueConverter<uint, SolidColorBrush>(color => new SolidColorBrush(color));

    public static readonly CornerRadiusFilterConverter TopCornerRadiusFilter =
        new()
        {
            Filter = Corners.TopLeft | Corners.TopRight
        };

    public static readonly CornerRadiusFilterConverter RightCornerRadiusFilter =
        new()
        {
            Filter = Corners.TopRight | Corners.BottomRight
        };

    public static readonly CornerRadiusFilterConverter BottomCornerRadiusFilter =
        new()
        {
            Filter = Corners.BottomLeft | Corners.BottomRight
        };

    public static readonly CornerRadiusFilterConverter LeftCornerRadiusFilter =
        new()
        {
            Filter = Corners.TopLeft | Corners.BottomLeft
        };

    public static readonly CornerRadiusToDoubleConverter TopLeftCornerRadius =
        new()
        {
            Corner = Corners.TopLeft
        };

    public static readonly CornerRadiusToDoubleConverter BottomRightCornerRadius =
        new()
        {
            Corner = Corners.BottomRight
        };

    public static readonly PlatformKeyGestureConverter KeyGesture = new();

    public static readonly MarginMultiplierConverter MarginMultiplier = new();
}