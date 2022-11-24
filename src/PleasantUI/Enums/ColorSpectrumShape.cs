using PleasantUI.Controls.Primitives;

namespace PleasantUI.Enums;

/// <summary>
/// Defines the shape of a <see cref="ColorSpectrum"/>.
/// </summary>
public enum ColorSpectrumShape
{
    /// <summary>
    /// The spectrum is in the shape of a rectangular or square box.
    /// Note that more colors are visible to the user in Box shape.
    /// </summary>
    Box,

    /// <summary>
    /// The spectrum is in the shape of an ellipse or circle.
    /// </summary>
    Ring,
};