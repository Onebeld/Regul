﻿using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using PleasantUI.Controls.Primitives;
using PleasantUI.Enums;

namespace PleasantUI.Controls;

public sealed partial class ColorView
{
    /// <summary>
    /// Defines the <see cref="Color"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<ColorView, Color>(
            nameof(Color),
            Colors.White,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="ColorSpectrumComponents"/> property.
    /// </summary>
    public static readonly StyledProperty<ColorSpectrumComponents> ColorSpectrumComponentsProperty =
        AvaloniaProperty.Register<ColorView, ColorSpectrumComponents>(
            nameof(ColorSpectrumComponents),
            ColorSpectrumComponents.HueSaturation);

    /// <summary>
    /// Defines the <see cref="ColorSpectrumShape"/> property.
    /// </summary>
    public static readonly StyledProperty<ColorSpectrumShape> ColorSpectrumShapeProperty =
        AvaloniaProperty.Register<ColorView, ColorSpectrumShape>(
            nameof(ColorSpectrumShape));

    /// <summary>
    /// Defines the <see cref="ColorModel"/> property.
    /// </summary>
    public static readonly StyledProperty<ColorModel> ColorModelProperty =
        AvaloniaProperty.Register<ColorView, ColorModel>(
            nameof(ColorModel),
            ColorModel.Rgba);

    /// <summary>
    /// Defines the <see cref="CustomPaletteColors"/> property.
    /// </summary>
    public static readonly DirectProperty<ColorView, ObservableCollection<Color>> CustomPaletteColorsProperty =
        AvaloniaProperty.RegisterDirect<ColorView, ObservableCollection<Color>>(
            nameof(CustomPaletteColors),
            o => o.CustomPaletteColors);

    /// <summary>
    /// Defines the <see cref="CustomPaletteColumnCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> CustomPaletteColumnCountProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(CustomPaletteColumnCount),
            4);

    /// <summary>
    /// Defines the <see cref="HsvColor"/> property.
    /// </summary>
    public static readonly StyledProperty<HsvColor> HsvColorProperty =
        AvaloniaProperty.Register<ColorView, HsvColor>(
            nameof(HsvColor),
            Colors.White.ToHsv(),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="IsAlphaEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsAlphaEnabled));

    /// <summary>
    /// Defines the <see cref="IsAlphaSliderVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaSliderVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsAlphaSliderVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsAlphaTextInputVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaTextInputVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsAlphaTextInputVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsColorChannelTextInputVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsColorChannelTextInputVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsColorChannelTextInputVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsColorPaletteVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsColorPaletteVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsColorPaletteVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsColorPreviewVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsColorPreviewVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsColorPreviewVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsColorSliderVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsColorSliderVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsColorSliderVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsColorSpectrumVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsColorSpectrumVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsColorSpectrumVisible),
            true);

    /// <summary>
    /// Defines the <see cref="IsHexInputVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHexInputVisibleProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(IsHexInputVisible),
            true);

    /// <summary>
    /// Defines the <see cref="MaxHue"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxHueProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(MaxHue),
            359);

    /// <summary>
    /// Defines the <see cref="MaxSaturation"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxSaturationProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(MaxSaturation),
            100);

    /// <summary>
    /// Defines the <see cref="MaxValue"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(MaxValue),
            100);

    /// <summary>
    /// Defines the <see cref="MinHue"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MinHueProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(MinHue));

    /// <summary>
    /// Defines the <see cref="MinSaturation"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MinSaturationProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(MinSaturation));

    /// <summary>
    /// Defines the <see cref="MinValue"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MinValueProperty =
        AvaloniaProperty.Register<ColorView, int>(
            nameof(MinValue));

    /// <summary>
    /// Defines the <see cref="ShowAccentColors"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowAccentColorsProperty =
        AvaloniaProperty.Register<ColorView, bool>(
            nameof(ShowAccentColors),
            true);

    public static readonly StyledProperty<bool> IsColorModeRgbaProperty =
        AvaloniaProperty.Register<ColorView, bool>(nameof(IsColorModeRgba), true);

    public static readonly StyledProperty<bool> IsColorModeHsvaProperty =
        AvaloniaProperty.Register<ColorView, bool>(nameof(IsColorModeHsva));

    public bool IsColorModeRgba
    {
        get => GetValue(IsColorModeRgbaProperty);
        set => SetValue(IsColorModeRgbaProperty, value);
    }

    public bool IsColorModeHsva
    {
        get => GetValue(IsColorModeHsvaProperty);
        set => SetValue(IsColorModeHsvaProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.Color"/>
    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.Components"/>
    public ColorSpectrumComponents ColorSpectrumComponents
    {
        get => GetValue(ColorSpectrumComponentsProperty);
        set => SetValue(ColorSpectrumComponentsProperty, value);
    }

    /// <inheritdoc cref="Shape"/>
    public ColorSpectrumShape ColorSpectrumShape
    {
        get => GetValue(ColorSpectrumShapeProperty);
        set => SetValue(ColorSpectrumShapeProperty, value);
    }

    /// <inheritdoc cref="ColorSlider.ColorModel"/>
    /// <remarks>
    /// This property is only applicable to the components tab.
    /// The spectrum tab must always be in HSV and the palette tab is pre-defined colors.
    /// </remarks>
    public ColorModel ColorModel
    {
        get => GetValue(ColorModelProperty);
        set => SetValue(ColorModelProperty, value);
    }

    /// <summary>
    /// Gets the list of custom palette colors.
    /// </summary>
    public ObservableCollection<Color> CustomPaletteColors => _customPaletteColors;

    /// <summary>
    /// Gets or sets the number of colors in each row (section) of the custom color palette.
    /// Within a standard palette, rows are shades and columns are colors.
    /// </summary>
    public int CustomPaletteColumnCount
    {
        get => GetValue(CustomPaletteColumnCountProperty);
        set => SetValue(CustomPaletteColumnCountProperty, value);
    }

    /// <inheritdoc cref="Avalonia.Media.HsvColor"/>
    public HsvColor HsvColor
    {
        get => GetValue(HsvColorProperty);
        set => SetValue(HsvColorProperty, value);
    }

    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    public bool IsAlphaSliderVisible
    {
        get => GetValue(IsAlphaSliderVisibleProperty);
        set => SetValue(IsAlphaSliderVisibleProperty, value);
    }

    public bool IsAlphaTextInputVisible
    {
        get => GetValue(IsAlphaTextInputVisibleProperty);
        set => SetValue(IsAlphaTextInputVisibleProperty, value);
    }

    public bool IsColorChannelTextInputVisible // TODO: Component
    {
        get => GetValue(IsColorChannelTextInputVisibleProperty);
        set => SetValue(IsColorChannelTextInputVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the color palette is visible.
    /// </summary>
    public bool IsColorPaletteVisible
    {
        get => GetValue(IsColorPaletteVisibleProperty);
        set => SetValue(IsColorPaletteVisibleProperty, value);
    }

    public bool IsColorPreviewVisible
    {
        get => GetValue(IsColorPreviewVisibleProperty);
        set => SetValue(IsColorPreviewVisibleProperty, value);
    }

    // IsColorComponentsVisible

    public bool IsColorSliderVisible // ColorSpectrumSlider
    {
        get => GetValue(IsColorSliderVisibleProperty);
        set => SetValue(IsColorSliderVisibleProperty, value);
    }

    public bool IsColorSpectrumVisible
    {
        get => GetValue(IsColorSpectrumVisibleProperty);
        set => SetValue(IsColorSpectrumVisibleProperty, value);
    }

    public bool IsHexInputVisible
    {
        get => GetValue(IsHexInputVisibleProperty);
        set => SetValue(IsHexInputVisibleProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.MaxHue"/>
    public int MaxHue
    {
        get => GetValue(MaxHueProperty);
        set => SetValue(MaxHueProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.MaxSaturation"/>
    public int MaxSaturation
    {
        get => GetValue(MaxSaturationProperty);
        set => SetValue(MaxSaturationProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.MaxValue"/>
    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.MinHue"/>
    public int MinHue
    {
        get => GetValue(MinHueProperty);
        set => SetValue(MinHueProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.MinSaturation"/>
    public int MinSaturation
    {
        get => GetValue(MinSaturationProperty);
        set => SetValue(MinSaturationProperty, value);
    }

    /// <inheritdoc cref="ColorSpectrum.MinValue"/>
    public int MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    /// <inheritdoc cref="ColorPreviewer.ShowAccentColors"/>
    public bool ShowAccentColors
    {
        get => GetValue(ShowAccentColorsProperty);
        set => SetValue(ShowAccentColorsProperty, value);
    }
}