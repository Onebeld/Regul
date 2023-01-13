using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Xaml.Interactivity;

/// <summary>
/// Interface implemented by all custom behaviors.
/// </summary>
public interface IBehavior
{
    /// <summary>
    /// Gets the <see cref="IAvaloniaObject"/> to which the <seealso cref="IBehavior"/> is attached.
    /// </summary>
    Control? AssociatedObject { get; }

    /// <summary>
    /// Attaches to the specified object.
    /// </summary>
    /// <param name="associatedObject">The <see cref="IAvaloniaObject"/> to which the <seealso cref="IBehavior"/> will be attached.</param>
    void Attach(AvaloniaObject? associatedObject);

    /// <summary>
    /// Detaches this instance from its associated object.
    /// </summary>
    void Detach();
}