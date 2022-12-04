using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using PleasantUI.Converters;
using PleasantUI.Helpers;

namespace PleasantUI.Controls.Primitives;

/// <summary>
/// Presents a preview color with optional accent colors.
/// </summary>
[TemplatePart(Name = nameof(_accentDec1Border), Type = typeof(Border))]
[TemplatePart(Name = nameof(_accentDec2Border), Type = typeof(Border))]
[TemplatePart(Name = nameof(_accentInc1Border), Type = typeof(Border))]
[TemplatePart(Name = nameof(_accentInc2Border), Type = typeof(Border))]
public partial class ColorPreviewer : TemplatedControl
{
    /// <summary>
    /// Event for when the selected color changes within the previewer.
    /// This occurs when an accent color is pressed.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    private bool _eventsConnected;

    private Border _accentDec1Border = null!;
    private Border _accentDec2Border = null!;
    private Border _accentInc1Border = null!;
    private Border _accentInc2Border = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPreviewer"/> class.
    /// </summary>
    public ColorPreviewer()
    {
    }

    /// <summary>
    /// Connects or disconnects all control event handlers.
    /// </summary>
    /// <param name="connected">True to connect event handlers, otherwise false.</param>
    private void ConnectEvents(bool connected)
    {
        if (connected && _eventsConnected == false)
        {
            // Add all events
            _accentDec1Border.PointerPressed += AccentBorder_PointerPressed;
            _accentDec2Border.PointerPressed += AccentBorder_PointerPressed;
            _accentInc1Border.PointerPressed += AccentBorder_PointerPressed;
            _accentInc2Border.PointerPressed += AccentBorder_PointerPressed;

            _eventsConnected = true;
        }
        else if (connected == false && _eventsConnected)
        {
            // Remove all events
            _accentDec1Border.PointerPressed -= AccentBorder_PointerPressed;
            _accentDec2Border.PointerPressed -= AccentBorder_PointerPressed;
            _accentInc1Border.PointerPressed -= AccentBorder_PointerPressed;
            _accentInc2Border.PointerPressed -= AccentBorder_PointerPressed;

            _eventsConnected = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // Remove any existing events present if the control was previously loaded then unloaded
        ConnectEvents(false);

        _accentDec1Border = e.NameScope.Find<Border>(nameof(_accentDec1Border))!;
        _accentDec2Border = e.NameScope.Find<Border>(nameof(_accentDec2Border))!;
        _accentInc1Border = e.NameScope.Find<Border>(nameof(_accentInc1Border))!;
        _accentInc2Border = e.NameScope.Find<Border>(nameof(_accentInc2Border))!;

        // Must connect after controls are found
        ConnectEvents(true);

        base.OnApplyTemplate(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == HsvColorProperty)
        {
            OnColorChanged(new ColorChangedEventArgs(
                change.GetOldValue<HsvColor>().ToRgb(),
                change.GetNewValue<HsvColor>().ToRgb()));
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

    /// <summary>
    /// Event handler for when an accent color border is pressed.
    /// This will update the color to the background of the pressed panel.
    /// </summary>
    private void AccentBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Border? border = sender as Border;
        int accentStep = 0;
        HsvColor hsvColor = HsvColor;

        // Get the value component delta
        try
        {
            accentStep = int.Parse(border?.Tag?.ToString() ?? "", CultureInfo.InvariantCulture);
        }
        catch
        {
            // ignored
        }

        HsvColor newHsvColor = AccentColorConverter.GetAccent(hsvColor, accentStep);
        HsvColor oldHsvColor = HsvColor;

        HsvColor = newHsvColor;
        OnColorChanged(new ColorChangedEventArgs(oldHsvColor.ToRgb(), newHsvColor.ToRgb()));
    }
}