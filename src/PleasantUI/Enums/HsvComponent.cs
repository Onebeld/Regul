using Avalonia.Media;

namespace PleasantUI.Enums;

/// <summary>
/// Defines a specific component in the HSV color model.
/// </summary>
public enum HsvComponent
{
    /// <summary>
    /// The Alpha component.
    /// </summary>
    /// <remarks>
    /// Also see: <see cref="HsvColor.A"/>
    /// </remarks>
    Alpha = 0,

    /// <summary>
    /// The Hue component.
    /// </summary>
    /// <remarks>
    /// Also see: <see cref="HsvColor.H"/>
    /// </remarks>
    Hue = 1,

    /// <summary>
    /// The Saturation component.
    /// </summary>
    /// <remarks>
    /// Also see: <see cref="HsvColor.S"/>
    /// </remarks>
    Saturation = 2,

    /// <summary>
    /// The Value component.
    /// </summary>
    /// <remarks>
    /// Also see: <see cref="HsvColor.V"/>
    /// </remarks>
    Value = 3
}