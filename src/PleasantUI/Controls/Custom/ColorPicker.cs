﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Media.Colors;

namespace PleasantUI.Controls.Custom
{
    public static class ColorHelpers
    {
        private static readonly Regex s_hexRegex = new Regex("^#[a-fA-F0-9]{8}$");

        public static bool IsValidHexColor(string hex)
        {
            return !string.IsNullOrWhiteSpace(hex) && s_hexRegex.Match(hex).Success;
        }

        public static string ToHexColor(Color color)
        {
            return $"#{color.ToUint32():X8}";
        }

        //public static Color FromHexColor(string hex) => Color.Parse(hex);

        public static void FromColor(Color color, out double h, out double s, out double v, out double a)
        {
            HSV hsv = new RGB(color.R, color.G, color.B).ToHSV();
            h = hsv.H;
            s = hsv.S;
            v = hsv.V;
            a = color.A * 100.0 / 255.0;
        }

        public static Color FromHSVA(double h, double s, double v, double a)
        {
            RGB rgb = new HSV(h, s, v).ToRGB();
            return new Color((byte)(a * 255.0 * 0.01), (byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
        }

        public static Color FromRGBA(byte r, byte g, byte b, double a)
        {
            return new Color((byte)(a * 255.0 * 0.01), r, g, b);
        }
    }

    public class UIntToBrushConverter : IValueConverter
    {
        public static readonly UIntToBrushConverter Instance = new UIntToBrushConverter();
    
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is uint u1))
                return AvaloniaProperty.UnsetValue;

            return new SolidColorBrush(u1);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ISolidColorBrush brush))
                return 0;

            return brush.Color.ToUint32();
        }
    }

    public class HexToBrushConverter : IValueConverter
    {
        public static readonly HexToBrushConverter Instance = new HexToBrushConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && targetType == typeof(IBrush))
                try
                {
                    if (ColorHelpers.IsValidHexColor(s)) return new SolidColorBrush { Color = Color.Parse(s) };
                }
                catch (Exception)
                {
                    return AvaloniaProperty.UnsetValue;
                }

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IBrush c && targetType == typeof(string))
                try
                {
                    return ((SolidColorBrush)c).Color.ToString();
                }
                catch (Exception)
                {
                    return AvaloniaProperty.UnsetValue;
                }

            return AvaloniaProperty.UnsetValue;
        }
    }

    public class HueConverter : IValueConverter
    {
        public static readonly HueConverter Instance = new HueConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double))
                return v * range / 360.0;

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double))
                return v * 360.0 / range;

            return AvaloniaProperty.UnsetValue;
        }
    }

    public class SaturationConverter : IValueConverter
    {
        public static readonly SaturationConverter Instance = new SaturationConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double)) return v * range * 0.01;

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double))
                return v * 100.0 / range;

            return AvaloniaProperty.UnsetValue;
        }
    }

    public class ValueConverter : IValueConverter
    {
        public static readonly ValueConverter Instance = new ValueConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double))
                return range - v * (range * 0.01);

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double))
                return 100.0 - v * (100.0 / range);

            return AvaloniaProperty.UnsetValue;
        }
    }

    public class AlphaConverter : IValueConverter
    {
        public static readonly AlphaConverter Instance = new AlphaConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double)) return v * range * 0.01;

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is double range && targetType == typeof(double))
                return v * 100.0 / range;

            return AvaloniaProperty.UnsetValue;
        }
    }

    public class HsvaToColorConverter : IMultiValueConverter
    {
        public static readonly HsvaToColorConverter Instance = new HsvaToColorConverter();
        
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            double[] v = values.OfType<double>().ToArray();
            if (v.Length == values.Count) return ColorHelpers.FromHSVA(v[0], v[1], v[2], v[3]);

            return AvaloniaProperty.UnsetValue;
        }
    }

    public class HueToColorConverter : IValueConverter
    {
        public static readonly HueToColorConverter Instance = new HueToColorConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double h && targetType == typeof(Color)) return ColorHelpers.FromHSVA(h, 100, 100, 100);

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && targetType == typeof(double))
                try
                {
                    if (ColorHelpers.IsValidHexColor(s))
                    {
                        ColorHelpers.FromColor(Color.Parse(s), out double h, out _, out _, out _);
                        return h;
                    }
                }
                catch (Exception)
                {
                    return AvaloniaProperty.UnsetValue;
                }

            return AvaloniaProperty.UnsetValue;
        }
    }

    public abstract class ColorPickerProperties : AvaloniaObject
    {
        public static readonly StyledProperty<ColorPicker> ColorPickerProperty =
            AvaloniaProperty.Register<ColorPickerProperties, ColorPicker>(nameof(ColorPicker));

        protected ColorPickerProperties()
        {
            this.GetObservable(ColorPickerProperty).Subscribe(x => OnColorPickerChange());
        }

        public ColorPicker ColorPicker
        {
            get => GetValue(ColorPickerProperty);
            set => SetValue(ColorPickerProperty, value);
        }

        public abstract void UpdateColorPickerValues();

        public abstract void UpdatePropertyValues();

        public virtual void OnColorPickerChange()
        {
            if (ColorPicker != null)
            {
                ColorPicker.GetObservable(ColorPicker.Value1Property).Subscribe(x => UpdatePropertyValues());
                ColorPicker.GetObservable(ColorPicker.Value2Property).Subscribe(x => UpdatePropertyValues());
                ColorPicker.GetObservable(ColorPicker.Value3Property).Subscribe(x => UpdatePropertyValues());
                ColorPicker.GetObservable(ColorPicker.Value4Property).Subscribe(x => UpdatePropertyValues());
            }
        }
    }

    public class HsvProperties : ColorPickerProperties
    {
        public static readonly StyledProperty<double> HueProperty =
            AvaloniaProperty.Register<HsvProperties, double>(nameof(Hue), 0.0, validate: ValidateHue);

        public static readonly StyledProperty<double> SaturationProperty =
            AvaloniaProperty.Register<HsvProperties, double>(nameof(Saturation), 100.0, validate: ValidateSaturation);

        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<HsvProperties, double>(nameof(Value), 100.0, validate: ValidateValue);

        private bool _updating;

        public HsvProperties()
        {
            this.GetObservable(HueProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(SaturationProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(ValueProperty).Subscribe(x => UpdateColorPickerValues());
        }

        public double Hue
        {
            get => GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public double Saturation
        {
            get => GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static bool ValidateHue(double hue)
        {
            if (hue < 0.0 || hue > 360.0) return false;

            return true;
        }

        private static bool ValidateSaturation(double saturation)
        {
            if (saturation < 0.0 || saturation > 100.0) return false;

            return true;
        }

        private static bool ValidateValue(double value)
        {
            if (value < 0.0 || value > 100.0) return false;

            return true;
        }

        public override void UpdateColorPickerValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                ColorPicker.Value1 = Hue;
                ColorPicker.Value2 = Saturation;
                ColorPicker.Value3 = Value;
                _updating = false;
            }
        }

        public override void UpdatePropertyValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                Hue = ColorPicker.Value1;
                Saturation = ColorPicker.Value2;
                Value = ColorPicker.Value3;
                _updating = false;
            }
        }
    }

    public class RgbProperties : ColorPickerProperties
    {
        public static readonly StyledProperty<byte> RedProperty =
            AvaloniaProperty.Register<RgbProperties, byte>(nameof(Red), 0xFF, validate: ValidateRed);

        public static readonly StyledProperty<byte> GreenProperty =
            AvaloniaProperty.Register<RgbProperties, byte>(nameof(Green), 0x00, validate: ValidateGreen);

        public static readonly StyledProperty<byte> BlueProperty =
            AvaloniaProperty.Register<RgbProperties, byte>(nameof(Blue), 0x00, validate: ValidateBlue);

        private bool _updating;

        public RgbProperties()
        {
            this.GetObservable(RedProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(GreenProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(BlueProperty).Subscribe(x => UpdateColorPickerValues());
        }

        public byte Red
        {
            get => GetValue(RedProperty);
            set => SetValue(RedProperty, value);
        }

        public byte Green
        {
            get => GetValue(GreenProperty);
            set => SetValue(GreenProperty, value);
        }

        public byte Blue
        {
            get => GetValue(BlueProperty);
            set => SetValue(BlueProperty, value);
        }

        private static bool ValidateRed(byte red)
        {
            if (red > 255) return false;

            return true;
        }

        private static bool ValidateGreen(byte green)
        {
            if (green > 255) return false;

            return true;
        }

        private static bool ValidateBlue(byte blue)
        {
            if (blue > 255) return false;

            return true;
        }

        public override void UpdateColorPickerValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                RGB rgb = new RGB(Red, Green, Blue);
                HSV hsv = rgb.ToHSV();
                ColorPicker.Value1 = hsv.H;
                ColorPicker.Value2 = hsv.S;
                ColorPicker.Value3 = hsv.V;
                _updating = false;
            }
        }

        public override void UpdatePropertyValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                HSV hsv = new HSV(ColorPicker.Value1, ColorPicker.Value2, ColorPicker.Value3);
                RGB rgb = hsv.ToRGB();
                Red = (byte)rgb.R;
                Green = (byte)rgb.G;
                Blue = (byte)rgb.B;
                _updating = false;
            }
        }
    }

    public class CmykProperties : ColorPickerProperties
    {
        public static readonly StyledProperty<double> CyanProperty =
            AvaloniaProperty.Register<CmykProperties, double>(nameof(Cyan), 0.0, validate: ValidateCyan);

        public static readonly StyledProperty<double> MagentaProperty =
            AvaloniaProperty.Register<CmykProperties, double>(nameof(Magenta), 100.0, validate: ValidateMagenta);

        public static readonly StyledProperty<double> YellowProperty =
            AvaloniaProperty.Register<CmykProperties, double>(nameof(Yellow), 100.0, validate: ValidateYellow);

        public static readonly StyledProperty<double> BlackKeyProperty =
            AvaloniaProperty.Register<CmykProperties, double>(nameof(BlackKey), 0.0, validate: ValidateBlackKey);

        private bool _updating;

        public CmykProperties()
        {
            this.GetObservable(CyanProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(MagentaProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(YellowProperty).Subscribe(x => UpdateColorPickerValues());
            this.GetObservable(BlackKeyProperty).Subscribe(x => UpdateColorPickerValues());
        }

        public double Cyan
        {
            get => GetValue(CyanProperty);
            set => SetValue(CyanProperty, value);
        }

        public double Magenta
        {
            get => GetValue(MagentaProperty);
            set => SetValue(MagentaProperty, value);
        }

        public double Yellow
        {
            get => GetValue(YellowProperty);
            set => SetValue(YellowProperty, value);
        }

        public double BlackKey
        {
            get => GetValue(BlackKeyProperty);
            set => SetValue(BlackKeyProperty, value);
        }

        private static bool ValidateCyan(double cyan)
        {
            if (cyan < 0.0 || cyan > 100.0) return false;

            return true;
        }

        private static bool ValidateMagenta(double magenta)
        {
            if (magenta < 0.0 || magenta > 100.0) return false;

            return true;
        }

        private static bool ValidateYellow(double yellow)
        {
            if (yellow < 0.0 || yellow > 100.0) return false;

            return true;
        }

        private static bool ValidateBlackKey(double blackKey)
        {
            if (blackKey < 0.0 || blackKey > 100.0) return false;

            return true;
        }

        public override void UpdateColorPickerValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                CMYK cmyk = new CMYK(Cyan, Magenta, Yellow, BlackKey);
                HSV hsv = cmyk.ToHSV();
                ColorPicker.Value1 = hsv.H;
                ColorPicker.Value2 = hsv.S;
                ColorPicker.Value3 = hsv.V;
                _updating = false;
            }
        }

        public override void UpdatePropertyValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                HSV hsv = new HSV(ColorPicker.Value1, ColorPicker.Value2, ColorPicker.Value3);
                CMYK cmyk = hsv.ToCMYK();
                Cyan = cmyk.C;
                Magenta = cmyk.M;
                Yellow = cmyk.Y;
                BlackKey = cmyk.K;
                _updating = false;
            }
        }
    }

    public class HexProperties : ColorPickerProperties
    {
        public static readonly StyledProperty<string> HexProperty =
            AvaloniaProperty.Register<HexProperties, string>(nameof(Hex), "#FFFF0000", validate: ValidateHex);

        private bool _updating;

        public HexProperties()
        {
            this.GetObservable(HexProperty).Subscribe(x => UpdateColorPickerValues());
        }

        public string Hex
        {
            get => GetValue(HexProperty);
            set => SetValue(HexProperty, value);
        }

        private static bool ValidateHex(string hex)
        {
            if (!ColorHelpers.IsValidHexColor(hex)) return false;

            return true;
        }

        public override void UpdateColorPickerValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                Color color = Color.Parse(Hex);
                ColorHelpers.FromColor(color, out double h, out double s, out double v, out double a);
                ColorPicker.Value1 = h;
                ColorPicker.Value2 = s;
                ColorPicker.Value3 = v;
                ColorPicker.Value4 = a;
                _updating = false;
            }
        }

        public override void UpdatePropertyValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                Color color = ColorHelpers.FromHSVA(ColorPicker.Value1, ColorPicker.Value2, ColorPicker.Value3,
                    ColorPicker.Value4);
                Hex = ColorHelpers.ToHexColor(color);
                _updating = false;
            }
        }
    }

    public class AlphaProperties : ColorPickerProperties
    {
        public static readonly StyledProperty<double> AlphaProperty =
            AvaloniaProperty.Register<AlphaProperties, double>(nameof(Alpha), 100.0, validate: ValidateAlpha);

        private bool _updating;

        public AlphaProperties()
        {
            this.GetObservable(AlphaProperty).Subscribe(x => UpdateColorPickerValues());
        }

        public double Alpha
        {
            get => GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        private static bool ValidateAlpha(double alpha)
        {
            if (alpha < 0.0 || alpha > 100.0) return false;

            return true;
        }

        public override void UpdateColorPickerValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                ColorPicker.Value4 = Alpha;
                _updating = false;
            }
        }

        public override void UpdatePropertyValues()
        {
            if (_updating == false && ColorPicker != null)
            {
                _updating = true;
                Alpha = ColorPicker.Value4;
                _updating = false;
            }
        }
    }

    public interface IValueConverters
    {
        IValueConverter Value1Converter { get; }
        IValueConverter Value2Converter { get; }
        IValueConverter Value3Converter { get; }
        IValueConverter Value4Converter { get; }
    }

    public class HsvValueConverters : IValueConverters
    {
        public IValueConverter Value1Converter { get; } = new HueConverter();

        public IValueConverter Value2Converter { get; } = new SaturationConverter();

        public IValueConverter Value3Converter { get; } = new ValueConverter();

        public IValueConverter Value4Converter { get; } = new AlphaConverter();
    }

    public class ColorPicker : TemplatedControl
    {
        public static readonly StyledProperty<double> Value1Property =
            AvaloniaProperty.Register<ColorPicker, double>(nameof(Value1));

        public static readonly StyledProperty<double> Value2Property =
            AvaloniaProperty.Register<ColorPicker, double>(nameof(Value2));

        public static readonly StyledProperty<double> Value3Property =
            AvaloniaProperty.Register<ColorPicker, double>(nameof(Value3));

        public static readonly StyledProperty<double> Value4Property =
            AvaloniaProperty.Register<ColorPicker, double>(nameof(Value4));

        public static readonly StyledProperty<Color> ColorProperty =
            AvaloniaProperty.Register<ColorPicker, Color>(nameof(Color));

        public static readonly RoutedEvent<RoutedEventArgs> ChangeColorEvent =
            RoutedEvent.Register<ColorPicker, RoutedEventArgs>(nameof(ChangeColor), RoutingStrategies.Bubble);

        private readonly IValueConverters _converters = new HsvValueConverters();
        private Canvas _alphaCanvas;
        private Thumb _alphaThumb;
        private bool _captured;

        private Canvas _colorCanvas;
        private Thumb _colorThumb;
        private Canvas _hueCanvas;
        private Thumb _hueThumb;
        private bool _updating;

        public ColorPicker()
        {
            this.GetObservable(Value1Property).Subscribe(x => OnValueChange());
            this.GetObservable(Value2Property).Subscribe(x => OnValueChange());
            this.GetObservable(Value3Property).Subscribe(x => OnValueChange());
            this.GetObservable(Value4Property).Subscribe(x => OnValueChange());
            this.GetObservable(ColorProperty).Subscribe(x => OnColorChange());
        }

        public double Value1
        {
            get => GetValue(Value1Property);
            set => SetValue(Value1Property, value);
        }

        public double Value2
        {
            get => GetValue(Value2Property);
            set => SetValue(Value2Property, value);
        }

        public double Value3
        {
            get => GetValue(Value3Property);
            set
            {
                double val = value;
                
                if (val < 0)
                    val = 0;
                
                SetValue(Value3Property, val);
            }
        }

        public double Value4
        {
            get => GetValue(Value4Property);
            set => SetValue(Value4Property, value);
        }

        public Color Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public event EventHandler<RoutedEventArgs> ChangeColor
        {
            add => AddHandler(ChangeColorEvent, value);
            remove => RemoveHandler(ChangeColorEvent, value);
        }

        protected virtual void OnChangeColor()
        {
            RoutedEventArgs e = new RoutedEventArgs(ChangeColorEvent);
            RaiseEvent(e);

            e.Handled = true;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (_colorCanvas != null)
            {
                _colorCanvas.PointerPressed -= ColorCanvas_PointerPressed;
                _colorCanvas.PointerReleased -= ColorCanvas_PointerReleased;
                _colorCanvas.PointerMoved -= ColorCanvas_PointerMoved;
            }

            if (_colorThumb != null) _colorThumb.DragDelta -= ColorThumb_DragDelta;

            if (_hueCanvas != null)
            {
                _hueCanvas.PointerPressed -= HueCanvas_PointerPressed;
                _hueCanvas.PointerReleased -= HueCanvas_PointerReleased;
                _hueCanvas.PointerMoved -= HueCanvas_PointerMoved;
            }

            if (_hueThumb != null) _hueThumb.DragDelta -= HueThumb_DragDelta;

            if (_alphaCanvas != null)
            {
                _alphaCanvas.PointerPressed -= AlphaCanvas_PointerPressed;
                _alphaCanvas.PointerReleased -= AlphaCanvas_PointerReleased;
                _alphaCanvas.PointerMoved -= AlphaCanvas_PointerMoved;
            }

            if (_alphaThumb != null) _alphaThumb.DragDelta -= AlphaThumb_DragDelta;

            _colorCanvas = e.NameScope.Find<Canvas>("PART_ColorCanvas");
            _colorThumb = e.NameScope.Find<Thumb>("PART_ColorThumb");
            _hueCanvas = e.NameScope.Find<Canvas>("PART_HueCanvas");
            _hueThumb = e.NameScope.Find<Thumb>("PART_HueThumb");
            _alphaCanvas = e.NameScope.Find<Canvas>("PART_AlphaCanvas");
            _alphaThumb = e.NameScope.Find<Thumb>("PART_AlphaThumb");

            if (_colorCanvas != null)
            {
                _colorCanvas.PointerPressed += ColorCanvas_PointerPressed;
                _colorCanvas.PointerReleased += ColorCanvas_PointerReleased;
                _colorCanvas.PointerMoved += ColorCanvas_PointerMoved;
            }

            if (_colorThumb != null) _colorThumb.DragDelta += ColorThumb_DragDelta;

            if (_hueCanvas != null)
            {
                _hueCanvas.PointerPressed += HueCanvas_PointerPressed;
                _hueCanvas.PointerReleased += HueCanvas_PointerReleased;
                _hueCanvas.PointerMoved += HueCanvas_PointerMoved;
            }

            if (_hueThumb != null) _hueThumb.DragDelta += HueThumb_DragDelta;

            if (_alphaCanvas != null)
            {
                _alphaCanvas.PointerPressed += AlphaCanvas_PointerPressed;
                _alphaCanvas.PointerReleased += AlphaCanvas_PointerReleased;
                _alphaCanvas.PointerMoved += AlphaCanvas_PointerMoved;
            }

            if (_alphaThumb != null) _alphaThumb.DragDelta += AlphaThumb_DragDelta;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size size = base.ArrangeOverride(finalSize);
            OnColorChange();
            return size;
        }

        private bool IsTemplateValid()
        {
            return _colorCanvas != null
                   && _colorThumb != null
                   && _hueCanvas != null
                   && _hueThumb != null
                   && _alphaCanvas != null
                   && _alphaThumb != null;
        }

        private double Clamp(double val, double min, double max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        private void MoveThumb(Canvas canvas, Thumb thumb, double x, double y)
        {
            if (canvas != null && thumb != null)
            {
                double left = Clamp(x, 0, canvas.Bounds.Width);
                double top = Clamp(y, 0, canvas.Bounds.Height);
                Canvas.SetLeft(thumb, left);
                Canvas.SetTop(thumb, top);
            }
        }

        private T Convert<T>(IValueConverter converter, T value, T range)
        {
            return (T)converter.Convert(value, typeof(T), range, CultureInfo.CurrentCulture);
        }

        private T ConvertBack<T>(IValueConverter converter, T value, T range)
        {
            return (T)converter.ConvertBack(value, typeof(T), range, CultureInfo.CurrentCulture);
        }

        private double GetValue1Range()
        {
            return _hueCanvas?.Bounds.Height ?? 0.0;
        }

        private double GetValue2Range()
        {
            return _colorCanvas?.Bounds.Width ?? 0.0;
        }

        private double GetValue3Range()
        {
            return _colorCanvas?.Bounds.Height ?? 0.0;
        }

        private double GetValue4Range()
        {
            return _alphaCanvas?.Bounds.Width ?? 0.0;
        }

        private void UpdateThumbsFromColor()
        {
            ColorHelpers.FromColor(Color, out double h, out double s, out double v, out double a);
            double hueY = Convert(_converters.Value1Converter, h, GetValue1Range());
            double colorX = Convert(_converters.Value2Converter, s, GetValue2Range());
            double colorY = Convert(_converters.Value3Converter, v, GetValue3Range());
            double alphaX = Convert(_converters.Value4Converter, a, GetValue4Range());
            MoveThumb(_hueCanvas, _hueThumb, 0, hueY);
            MoveThumb(_colorCanvas, _colorThumb, colorX, colorY);
            MoveThumb(_alphaCanvas, _alphaThumb, alphaX, 0);
        }

        private void UpdateThumbsFromValues()
        {
            double hueY = Convert(_converters.Value1Converter, Value1, GetValue1Range());
            double colorX = Convert(_converters.Value2Converter, Value2, GetValue2Range());
            double colorY = Convert(_converters.Value3Converter, Value3, GetValue3Range());
            double alphaX = Convert(_converters.Value4Converter, Value4, GetValue4Range());
            MoveThumb(_hueCanvas, _hueThumb, 0, hueY);
            MoveThumb(_colorCanvas, _colorThumb, colorX, colorY);
            MoveThumb(_alphaCanvas, _alphaThumb, alphaX, 0);
        }

        private void UpdateValuesFromThumbs()
        {
            double hueY = Canvas.GetTop(_hueThumb);
            double colorX = Canvas.GetLeft(_colorThumb);
            double colorY = Canvas.GetTop(_colorThumb);
            double alphaX = Canvas.GetLeft(_alphaThumb);
            Value1 = ConvertBack(_converters.Value1Converter, hueY, GetValue1Range());
            Value2 = ConvertBack(_converters.Value2Converter, colorX, GetValue2Range());
            Value3 = ConvertBack(_converters.Value3Converter, colorY, GetValue3Range());
            Value4 = ConvertBack(_converters.Value4Converter, alphaX, GetValue4Range());
            Color = ColorHelpers.FromHSVA(Value1, Value2, Value3, Value4);
        }

        private void UpdateColorFromThumbs()
        {
            double hueY = Canvas.GetTop(_hueThumb);
            double colorX = Canvas.GetLeft(_colorThumb);
            double colorY = Canvas.GetTop(_colorThumb);
            double alphaX = Canvas.GetLeft(_alphaThumb);
            double h = ConvertBack(_converters.Value1Converter, hueY, GetValue1Range());
            double s = ConvertBack(_converters.Value2Converter, colorX, GetValue2Range());
            double v = ConvertBack(_converters.Value3Converter, colorY, GetValue3Range());
            double a = ConvertBack(_converters.Value4Converter, alphaX, GetValue4Range());
            Color = ColorHelpers.FromHSVA(h, s, v, a);
            OnChangeColor();
        }

        private void OnValueChange()
        {
            if (_updating == false && IsTemplateValid())
            {
                _updating = true;
                UpdateThumbsFromValues();
                UpdateValuesFromThumbs();
                UpdateColorFromThumbs();
                _updating = false;
            }
        }

        private void OnColorChange()
        {
            if (_updating == false && IsTemplateValid())
            {
                _updating = true;
                UpdateThumbsFromColor();
                UpdateValuesFromThumbs();
                UpdateColorFromThumbs();
                _updating = false;
            }
        }

        private void ColorCanvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Point position = e.GetPosition(_colorCanvas);
            _updating = true;
            MoveThumb(_colorCanvas, _colorThumb, position.X, position.Y);
            UpdateValuesFromThumbs();
            UpdateColorFromThumbs();
            _updating = false;
            _captured = true;
        }

        private void ColorCanvas_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_captured) _captured = false;
        }

        private void ColorCanvas_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_captured)
            {
                Point position = e.GetPosition(_colorCanvas);
                _updating = true;
                MoveThumb(_colorCanvas, _colorThumb, position.X, position.Y);
                UpdateValuesFromThumbs();
                UpdateColorFromThumbs();
                OnChangeColor();
                _updating = false;
            }
        }

        private void ColorThumb_DragDelta(object sender, VectorEventArgs e)
        {
            double left = Canvas.GetLeft(_colorThumb);
            double top = Canvas.GetTop(_colorThumb);
            _updating = true;
            MoveThumb(_colorCanvas, _colorThumb, left + e.Vector.X, top + e.Vector.Y);
            UpdateValuesFromThumbs();
            UpdateColorFromThumbs();
            _updating = false;
        }

        private void HueCanvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Point position = e.GetPosition(_hueCanvas);
            _updating = true;
            MoveThumb(_hueCanvas, _hueThumb, 0, position.Y);
            UpdateValuesFromThumbs();
            UpdateColorFromThumbs();
            _updating = false;
            _captured = true;
        }

        private void HueCanvas_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_captured) _captured = false;
        }

        private void HueCanvas_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_captured)
            {
                Point position = e.GetPosition(_hueCanvas);
                _updating = true;
                MoveThumb(_hueCanvas, _hueThumb, 0, position.Y);
                UpdateValuesFromThumbs();
                UpdateColorFromThumbs();
                _updating = false;
            }
        }

        private void HueThumb_DragDelta(object sender, VectorEventArgs e)
        {
            double top = Canvas.GetTop(_hueThumb);
            _updating = true;
            MoveThumb(_hueCanvas, _hueThumb, 0, top + e.Vector.Y);
            UpdateValuesFromThumbs();
            UpdateColorFromThumbs();
            _updating = false;
        }

        private void AlphaCanvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Point position = e.GetPosition(_alphaCanvas);
            _updating = true;
            MoveThumb(_alphaCanvas, _alphaThumb, position.X, 0);
            UpdateValuesFromThumbs();
            UpdateColorFromThumbs();
            _updating = false;
            _captured = true;
        }

        private void AlphaCanvas_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_captured) _captured = false;
        }

        private void AlphaCanvas_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_captured)
            {
                Point position = e.GetPosition(_alphaCanvas);
                _updating = true;
                MoveThumb(_alphaCanvas, _alphaThumb, position.X, 0);
                UpdateValuesFromThumbs();
                UpdateColorFromThumbs();
                _updating = false;
            }
        }

        private void AlphaThumb_DragDelta(object sender, VectorEventArgs e)
        {
            double left = Canvas.GetLeft(_alphaThumb);
            _updating = true;
            MoveThumb(_alphaCanvas, _alphaThumb, left + e.Vector.X, 0);
            UpdateValuesFromThumbs();
            UpdateColorFromThumbs();
            _updating = false;
        }
    }
}