﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;
using PleasantUI.Enums;
using PleasantUI.Helpers;

namespace PleasantUI.Controls.Primitives;

/// <summary>
/// A slider with a background that represents a single color component.
/// </summary>
[PseudoClasses(PcDarkSelector, PcLightSelector)]
public partial class ColorSlider : Slider
{
    protected const string PcDarkSelector = ":dark-selector";
    protected const string PcLightSelector = ":light-selector";

    /// <summary>
    /// Event for when the selected color changes within the slider.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    /// <summary>
    /// Defines the maximum hue component value
    /// (other components are always 0..100 or 0.255).
    /// </summary>
    /// <remarks>
    /// This should match the default <see cref="ColorSpectrum.MaxHue"/> property.
    /// </remarks>
    private const double MaxHue = 359;

    private bool _disableUpdates;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorSlider"/> class.
    /// </summary>
    public ColorSlider()
    {
    }

    /// <summary>
    /// Updates the visual state of the control by applying latest PseudoClasses.
    /// </summary>
    private void UpdatePseudoClasses()
    {
        // The slider itself can be transparent for certain color values.
        // This causes an issue where a white selector thumb over a light window background or
        // a black selector thumb over a dark window background is not visible.
        // This means under a certain alpha threshold, neither a white or black selector thumb
        // should be shown and instead the default slider thumb color should be used instead.
        if (Color.A < 128 &&
            (IsAlphaMaxForced == false ||
             ColorComponent == ColorComponent.Alpha))
        {
            PseudoClasses.Set(PcDarkSelector, false);
            PseudoClasses.Set(PcLightSelector, false);
        }
        else
        {
            Color perceivedColor;

            if (ColorModel == ColorModel.Hsva)
            {
                perceivedColor = GetEquivalentBackgroundColor(HsvColor).ToRgb();
            }
            else
            {
                perceivedColor = GetEquivalentBackgroundColor(Color);
            }

            if (ColorHelper.GetRelativeLuminance(perceivedColor) <= 0.5)
            {
                PseudoClasses.Set(PcDarkSelector, false);
                PseudoClasses.Set(PcLightSelector, true);
            }
            else
            {
                PseudoClasses.Set(PcDarkSelector, true);
                PseudoClasses.Set(PcLightSelector, false);
            }
        }
    }

    /// <summary>
    /// Generates a new background image for the color slider and applies it.
    /// </summary>
    private async void UpdateBackground()
    {
        // In Avalonia, Bounds returns the actual device-independent pixel size of a control.
        // However, this is not necessarily the size of the control rendered on a display.
        // A desktop or application scaling factor may be applied which must be accounted for here.
        // Remember bitmaps in Avalonia are rendered mapping to actual device pixels, not the device-
        // independent pixels of controls.

        double scale = LayoutHelper.GetLayoutScale(this);
        int pixelWidth = Convert.ToInt32(Bounds.Width * scale);
        int pixelHeight = Convert.ToInt32(Bounds.Height * scale);

        if (pixelWidth != 0 && pixelHeight != 0)
        {
            byte[] bitmap = await ColorPickerHelpers.CreateComponentBitmapAsync(
                pixelWidth,
                pixelHeight,
                Orientation,
                ColorModel,
                ColorComponent,
                HsvColor,
                IsAlphaMaxForced,
                IsSaturationValueMaxForced);

            Background = new ImageBrush(ColorPickerHelpers.CreateBitmapFromPixelData(bitmap, pixelWidth, pixelHeight));

        }
    }

    /// <summary>
    /// Rounds the component values of the given <see cref="HsvColor"/>.
    /// This is useful for user-display and to ensure a color matches user selection exactly.
    /// </summary>
    /// <param name="hsvColor">The <see cref="HsvColor"/> to round component values for.</param>
    /// <returns>A new <see cref="HsvColor"/> with rounded component values.</returns>
    private HsvColor RoundComponentValues(HsvColor hsvColor)
    {
        return new HsvColor(
            Math.Round(hsvColor.A, 2, MidpointRounding.AwayFromZero),
            Math.Round(hsvColor.H, 0, MidpointRounding.AwayFromZero),
            Math.Round(hsvColor.S, 2, MidpointRounding.AwayFromZero),
            Math.Round(hsvColor.V, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Updates the slider property values by applying the current color.
    /// </summary>
    /// <remarks>
    /// Warning: This will trigger property changed updates.
    /// Consider using <see cref="_disableUpdates"/> externally.
    /// </remarks>
    private void SetColorToSliderValues()
    {
        HsvColor hsvColor = HsvColor;
        Color rgbColor = Color;
        ColorComponent component = ColorComponent;

        if (ColorModel == ColorModel.Hsva)
        {
            if (IsRoundingEnabled)
            {
                hsvColor = RoundComponentValues(hsvColor);
            }

            // Note: Components converted into a usable range for the user
            switch (component)
            {
                case ColorComponent.Alpha:
                    Minimum = 0;
                    Maximum = 100;
                    Value = hsvColor.A * 100;
                    break;
                case ColorComponent.Component1: // Hue
                    Minimum = 0;
                    Maximum = MaxHue;
                    Value = hsvColor.H;
                    break;
                case ColorComponent.Component2: // Saturation
                    Minimum = 0;
                    Maximum = 100;
                    Value = hsvColor.S * 100;
                    break;
                case ColorComponent.Component3: // Value
                    Minimum = 0;
                    Maximum = 100;
                    Value = hsvColor.V * 100;
                    break;
            }
        }
        else
        {
            switch (component)
            {
                case ColorComponent.Alpha:
                    Minimum = 0;
                    Maximum = 255;
                    Value = Convert.ToDouble(rgbColor.A);
                    break;
                case ColorComponent.Component1: // Red
                    Minimum = 0;
                    Maximum = 255;
                    Value = Convert.ToDouble(rgbColor.R);
                    break;
                case ColorComponent.Component2: // Green
                    Minimum = 0;
                    Maximum = 255;
                    Value = Convert.ToDouble(rgbColor.G);
                    break;
                case ColorComponent.Component3: // Blue
                    Minimum = 0;
                    Maximum = 255;
                    Value = Convert.ToDouble(rgbColor.B);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the current color determined by the slider values.
    /// </summary>
    private (Color, HsvColor) GetColorFromSliderValues()
    {
        HsvColor hsvColor = new();
        Color rgbColor = new();
        double sliderPercent = Value / (Maximum - Minimum);

        HsvColor baseHsvColor = HsvColor;
        Color baseRgbColor = Color;
        ColorComponent component = ColorComponent;

        if (ColorModel == ColorModel.Hsva)
        {
            switch (component)
            {
                case ColorComponent.Alpha:
                {
                    hsvColor = new HsvColor(sliderPercent, baseHsvColor.H, baseHsvColor.S, baseHsvColor.V);
                    break;
                }
                case ColorComponent.Component1:
                {
                    hsvColor = new HsvColor(baseHsvColor.A, sliderPercent * MaxHue, baseHsvColor.S, baseHsvColor.V);
                    break;
                }
                case ColorComponent.Component2:
                {
                    hsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, sliderPercent, baseHsvColor.V);
                    break;
                }
                case ColorComponent.Component3:
                {
                    hsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, baseHsvColor.S, sliderPercent);
                    break;
                }
            }

            rgbColor = hsvColor.ToRgb();
        }
        else
        {
            byte componentValue = Convert.ToByte(MathUtilities.Clamp(sliderPercent * 255, 0, 255));

            switch (component)
            {
                case ColorComponent.Alpha:
                    rgbColor = new Color(componentValue, baseRgbColor.R, baseRgbColor.G, baseRgbColor.B);
                    break;
                case ColorComponent.Component1:
                    rgbColor = new Color(baseRgbColor.A, componentValue, baseRgbColor.G, baseRgbColor.B);
                    break;
                case ColorComponent.Component2:
                    rgbColor = new Color(baseRgbColor.A, baseRgbColor.R, componentValue, baseRgbColor.B);
                    break;
                case ColorComponent.Component3:
                    rgbColor = new Color(baseRgbColor.A, baseRgbColor.R, baseRgbColor.G, componentValue);
                    break;
            }

            hsvColor = rgbColor.ToHsv();
        }

        if (IsRoundingEnabled)
        {
            hsvColor = RoundComponentValues(hsvColor);
        }

        return (rgbColor, hsvColor);
    }

    /// <summary>
    /// Gets the actual background color displayed for the given HSV color.
    /// This can differ due to the effects of certain properties intended to improve perception.
    /// </summary>
    /// <param name="hsvColor">The actual color to get the equivalent background color for.</param>
    /// <returns>The equivalent, perceived background color.</returns>
    private HsvColor GetEquivalentBackgroundColor(HsvColor hsvColor)
    {
        ColorComponent component = ColorComponent;
        bool isAlphaMaxForced = IsAlphaMaxForced;
        bool isSaturationValueMaxForced = IsSaturationValueMaxForced;

        if (isAlphaMaxForced &&
            component != ColorComponent.Alpha)
        {
            hsvColor = new HsvColor(1.0, hsvColor.H, hsvColor.S, hsvColor.V);
        }

        switch (component)
        {
            case ColorComponent.Component1:
                return new HsvColor(
                    hsvColor.A,
                    hsvColor.H,
                    isSaturationValueMaxForced ? 1.0 : hsvColor.S,
                    isSaturationValueMaxForced ? 1.0 : hsvColor.V);
            case ColorComponent.Component2:
                return new HsvColor(
                    hsvColor.A,
                    hsvColor.H,
                    hsvColor.S,
                    isSaturationValueMaxForced ? 1.0 : hsvColor.V);
            case ColorComponent.Component3:
                return new HsvColor(
                    hsvColor.A,
                    hsvColor.H,
                    isSaturationValueMaxForced ? 1.0 : hsvColor.S,
                    hsvColor.V);
            default:
                return hsvColor;
        }
    }

    /// <summary>
    /// Gets the actual background color displayed for the given RGB color.
    /// This can differ due to the effects of certain properties intended to improve perception.
    /// </summary>
    /// <param name="rgbColor">The actual color to get the equivalent background color for.</param>
    /// <returns>The equivalent, perceived background color.</returns>
    private Color GetEquivalentBackgroundColor(Color rgbColor)
    {
        ColorComponent component = ColorComponent;
        bool isAlphaMaxForced = IsAlphaMaxForced;

        if (isAlphaMaxForced &&
            component != ColorComponent.Alpha)
        {
            rgbColor = new Color(255, rgbColor.R, rgbColor.G, rgbColor.B);
        }

        return rgbColor;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_disableUpdates)
        {
            base.OnPropertyChanged(change);
            return;
        }

        // Always keep the two color properties in sync
        if (change.Property == ColorProperty)
        {
            _disableUpdates = true;

            HsvColor = Color.ToHsv();

            SetColorToSliderValues();
            UpdateBackground();
            UpdatePseudoClasses();

            OnColorChanged(new ColorChangedEventArgs(
                change.GetOldValue<Color>(),
                change.GetNewValue<Color>()));

            _disableUpdates = false;
        }
        else if (change.Property == ColorModelProperty)
        {
            _disableUpdates = true;

            SetColorToSliderValues();
            UpdateBackground();
            UpdatePseudoClasses();

            _disableUpdates = false;
        }
        else if (change.Property == HsvColorProperty)
        {
            _disableUpdates = true;

            Color = HsvColor.ToRgb();

            SetColorToSliderValues();
            UpdateBackground();
            UpdatePseudoClasses();

            OnColorChanged(new ColorChangedEventArgs(
                change.GetOldValue<HsvColor>().ToRgb(),
                change.GetNewValue<HsvColor>().ToRgb()));

            _disableUpdates = false;
        }
        else if (change.Property == IsRoundingEnabledProperty)
        {
            SetColorToSliderValues();
        }
        else if (change.Property == BoundsProperty)
        {
            UpdateBackground();
        }
        else if (change.Property == ValueProperty ||
                 change.Property == MinimumProperty ||
                 change.Property == MaximumProperty)
        {
            _disableUpdates = true;

            Color oldColor = Color;
            (Color color, HsvColor hsvColor) = GetColorFromSliderValues();

            if (ColorModel == ColorModel.Hsva)
            {
                HsvColor = hsvColor;
                Color = hsvColor.ToRgb();
            }
            else
            {
                Color = color;
                HsvColor = color.ToHsv();
            }

            UpdatePseudoClasses();
            OnColorChanged(new ColorChangedEventArgs(oldColor, Color));

            _disableUpdates = false;
        }

        base.OnPropertyChanged(change);
    }

    /// <summary>
    /// Called before the <see cref="ColorChanged"/> event occurs.
    /// </summary>
    /// <param name="e">The <see cref="ColorChangedEventArgs"/> defining old/new colors.</param>
    protected virtual void OnColorChanged(ColorChangedEventArgs e)
    {
        ColorChanged?.Invoke(this, e);
    }
}