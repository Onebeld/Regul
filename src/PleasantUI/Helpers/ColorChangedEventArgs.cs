﻿using Avalonia.Media;

namespace PleasantUI.Helpers;

/// <summary>
/// Holds the details of a ColorChanged event.
/// </summary>
/// <remarks>
/// HSV color information is intentionally not provided.
/// Use <see cref="Color.ToHsv()"/> to obtain it.
/// </remarks>
public class ColorChangedEventArgs : System.EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldColor">The old/original color from before the change event.</param>
    /// <param name="newColor">The new/updated color that triggered the change event.</param>
    public ColorChangedEventArgs(Color oldColor, Color newColor)
    {
        OldColor = oldColor;
        NewColor = newColor;
    }

    /// <summary>
    /// Gets the old/original color from before the change event.
    /// </summary>
    public Color OldColor { get; private set; }

    /// <summary>
    /// Gets the new/updated color that triggered the change event.
    /// </summary>
    public Color NewColor { get; private set; }
}