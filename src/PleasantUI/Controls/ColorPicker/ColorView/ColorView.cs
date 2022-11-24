using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Converters;
using PleasantUI.Helpers;

namespace PleasantUI.Controls;

/// <summary>
/// Presents a color for user editing using a spectrum, palette and component sliders.
/// </summary>
[TemplatePart("PART_HexTextBox", typeof(TextBoxWithSymbol))]
public sealed partial class ColorView : TemplatedControl
{
    /// <summary>
    /// Event for when the selected color changes within the slider.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    // XAML template parts
    private TextBoxWithSymbol? _hexTextBox;
    private Button? _addColorButton;
    private MenuItem? _deleteColor;

    private ObservableCollection<Color> _customPaletteColors = new();
    private ColorToHexConverter _colorToHexConverter = new();
    private bool _disableUpdates;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorView"/> class.
    /// </summary>
    public ColorView()
    {
    }
    
    /// <summary>
    /// Gets the value of the hex TextBox and sets it as the current <see cref="Color"/>.
    /// If invalid, the TextBox hex text will revert back to the last valid color.
    /// </summary>
    private void GetColorFromHexTextBox()
    {
        if (_hexTextBox != null)
        {
            object? convertedColor = _colorToHexConverter.ConvertBack(_hexTextBox.Text, typeof(Color), null, CultureInfo.CurrentCulture);

            if (convertedColor is Color color)
            {
                Color = color;
            }

            // Re-apply the hex value
            // This ensure the hex color value is always valid and formatted correctly
            _hexTextBox.Text = _colorToHexConverter.Convert(Color, typeof(string), null, CultureInfo.CurrentCulture) as string;
        }
    }

    /// <summary>
    /// Sets the current <see cref="Color"/> to the hex TextBox.
    /// </summary>
    private void SetColorToHexTextBox()
    {
        if (_hexTextBox != null)
        {
            _hexTextBox.Text = _colorToHexConverter.Convert(Color, typeof(string), null, CultureInfo.CurrentCulture) as string;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_hexTextBox != null)
        {
            _hexTextBox.KeyDown -= HexTextBox_KeyDown;
            _hexTextBox.LostFocus -= HexTextBox_LostFocus;
        }

        _hexTextBox = e.NameScope.Find<TextBoxWithSymbol>("PART_HexTextBox");
        _addColorButton = e.NameScope.Find<Button>("AddColorButton")!;
        _deleteColor = e.NameScope.Find<MenuItem>("DeleteColor");
        SetColorToHexTextBox();

        if (_hexTextBox != null)
        {
            _hexTextBox.KeyDown += HexTextBox_KeyDown;
            _hexTextBox.LostFocus += HexTextBox_LostFocus;
        }

        if (_deleteColor != null) _deleteColor.Click += DeleteColor;

        if (Color == Colors.White)
        {
            Color = Color.FromRgb(255, 255 ,254);
            Color = Color.FromRgb(255, 255 ,255);
        }
        
        _addColorButton.Click += AddColor;
        
        foreach (uint u in PleasantUiSettings.Instance.ColorPalette) 
            CustomPaletteColors.Add(Color.FromUInt32(u));

        base.OnApplyTemplate(e);
    }

    private void DeleteColor(object? sender, RoutedEventArgs e)
    {
        uint color = HsvColor.ToRgb().ToUint32();
        
        CustomPaletteColors.Remove(HsvColor.ToRgb());
        PleasantUiSettings.Instance.ColorPalette.Remove(color);
        
        Color = Color.FromUInt32(color);
    }

    private void AddColor(object? sender, RoutedEventArgs e)
    {
        uint color = HsvColor.ToRgb().ToUint32();
        
        if (PleasantUiSettings.Instance.ColorPalette.Contains(color)) return;
        CustomPaletteColors.Add(HsvColor.ToRgb());
        PleasantUiSettings.Instance.ColorPalette.Add(color);
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
            SetColorToHexTextBox();

            OnColorChanged(new ColorChangedEventArgs(
                change.GetOldValue<Color>(),
                change.GetNewValue<Color>()));

            _disableUpdates = false;
        }
        else if (change.Property == HsvColorProperty)
        {
            _disableUpdates = true;

            Color = HsvColor.ToRgb();
            SetColorToHexTextBox();

            OnColorChanged(new ColorChangedEventArgs(
                change.GetOldValue<HsvColor>().ToRgb(),
                change.GetNewValue<HsvColor>().ToRgb()));

            _disableUpdates = false;
        }

        base.OnPropertyChanged(change);
    }

    /// <summary>
    /// Called before the <see cref="ColorChanged"/> event occurs.
    /// </summary>
    /// <param name="e">The <see cref="ColorChangedEventArgs"/> defining old/new colors.</param>
    private void OnColorChanged(ColorChangedEventArgs e)
    {
        ColorChanged?.Invoke(this, e);
    }
    
    /// <summary>
    /// Event handler for when a key is pressed within the Hex RGB value TextBox.
    /// This is used to trigger re-evaluation of the color based on the TextBox value.
    /// </summary>
    private void HexTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) 
            GetColorFromHexTextBox();
    }

    /// <summary>
    /// Event handler for when the Hex RGB value TextBox looses focus.
    /// This is used to trigger re-evaluation of the color based on the TextBox value.
    /// </summary>
    private void HexTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        GetColorFromHexTextBox();
    }
}