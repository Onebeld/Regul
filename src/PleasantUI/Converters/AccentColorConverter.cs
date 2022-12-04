using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PleasantUI.Converters;

/// <summary>
/// Creates an accent color for a given base color value and step parameter.
/// </summary>
/// <remarks>
/// This is a highly-specialized converter for the color picker.
/// </remarks>
public class AccentColorConverter : IValueConverter
{
    private static readonly Lazy<AccentColorConverter> Lazy = new(() => new AccentColorConverter());
    public static AccentColorConverter Instance
    {
        get => Lazy.Value;
    }

    /// <summary>
    /// The amount to change the Value component for each accent color step.
    /// </summary>
    public const double ValueDelta = 0.1;

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int accentStep;
        Color? rgbColor = null;
        HsvColor? hsvColor = null;

        if (value is Color valueColor)
        {
            rgbColor = valueColor;
        }
        else if (value is HslColor valueHslColor)
        {
            rgbColor = valueHslColor.ToRgb();
        }
        else if (value is HsvColor valueHsvColor)
        {
            hsvColor = valueHsvColor;
        }
        else if (value is SolidColorBrush valueBrush)
        {
            rgbColor = valueBrush.Color;
        }
        else
        {
            // Invalid color value provided
            return AvaloniaProperty.UnsetValue;
        }

        // Get the value component delta
        try
        {
            accentStep = int.Parse(parameter?.ToString() ?? "", CultureInfo.InvariantCulture);
        }
        catch
        {
            // Invalid parameter provided, unable to convert to integer
            return AvaloniaProperty.UnsetValue;
        }

        if (hsvColor == null &&
            rgbColor != null)
        {
            hsvColor = rgbColor.Value.ToHsv();
        }

        if (hsvColor != null)
        {
            return new SolidColorBrush(GetAccent(hsvColor.Value, accentStep).ToRgb());
        }
        return AvaloniaProperty.UnsetValue;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }

    /// <summary>
    /// This does not account for perceptual differences and also does not match with
    /// system accent color calculation.
    /// </summary>
    /// <remarks>
    /// Use the HSV representation as it's more perceptual.
    /// In most cases only the value is changed by a fixed percentage so the algorithm is reproducible.
    /// </remarks>
    /// <param name="hsvColor">The base color to calculate the accent from.</param>
    /// <param name="accentStep">The number of accent color steps to move.</param>
    /// <returns>The new accent color.</returns>
    public static HsvColor GetAccent(HsvColor hsvColor, int accentStep)
    {
        if (accentStep == 0) return hsvColor;

        double colorValue = hsvColor.V;
        colorValue += accentStep * ValueDelta;
        colorValue = Math.Round(colorValue, 2);

        return new HsvColor(hsvColor.A, hsvColor.H, hsvColor.S, colorValue);
    }
}