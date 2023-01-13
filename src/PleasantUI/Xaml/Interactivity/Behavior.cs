using System.Globalization;
using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Xaml.Interactivity;

public class Behavior : AvaloniaObject, IBehavior
{
    /// <summary>
    /// Gets the <see cref="IAvaloniaObject"/> to which the behavior is attached.
    /// </summary>
    public Control? AssociatedObject { get; private set; }

    /// <summary>
    /// Attaches the behavior to the specified <see cref="IAvaloniaObject"/>.
    /// </summary>
    /// <param name="associatedObject">The <see cref="IAvaloniaObject"/> to which to attach.</param>
    /// <exception cref="ArgumentNullException"><paramref name="associatedObject"/> is null.</exception>
    public void Attach(AvaloniaObject? associatedObject)
    {
        if (Equals(associatedObject, AssociatedObject))
        {
            return;
        }

        if (AssociatedObject is { })
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture,
                "An instance of a behavior cannot be attached to more than one object at a time."));
        }
        AssociatedObject = associatedObject as Control ?? throw new ArgumentNullException(nameof(associatedObject));

        OnAttached();
    }

    /// <summary>
    /// Detaches the behaviors from the <see cref="AssociatedObject"/>.
    /// </summary>
    public void Detach()
    {
        OnDetaching();
        AssociatedObject = null;
    }

    /// <summary>
    /// Called after the behavior is attached to the <see cref="AssociatedObject"/>.
    /// </summary>
    /// <remarks>
    /// Override this to hook up functionality to the <see cref="AssociatedObject"/>
    /// </remarks>
    protected virtual void OnAttached()
    {
    }

    /// <summary>
    /// Called when the behavior is being detached from its <see cref="AssociatedObject"/>.
    /// </summary>
    /// <remarks>
    /// Override this to unhook functionality from the <see cref="AssociatedObject"/>
    /// </remarks>
    protected virtual void OnDetaching()
    {
    }

    internal void AttachedToVisualTree()
    {
        OnAttachedToVisualTree();
    }

    internal void DetachedFromVisualTree()
    {
        OnDetachedFromVisualTree();
    }

    /// <summary>
    /// Called after the <see cref="AssociatedObject"/> is attached to the visual tree.
    /// </summary>
    /// <remarks>
    /// Invoked only when the <see cref="AssociatedObject"/> is of type <see cref="IControl"/>.
    /// </remarks>
    protected virtual void OnAttachedToVisualTree()
    {
    }

    /// <summary>
    /// Called when the <see cref="AssociatedObject"/> is being detached from the visual tree.
    /// </summary>
    /// <remarks>
    /// Invoked only when the <see cref="AssociatedObject"/> is of type <see cref="IControl"/>.
    /// </remarks>
    protected virtual void OnDetachedFromVisualTree()
    {
    }
}

/// <summary>
/// A base class for behaviors making them code compatible with older frameworks,
/// and allow for typed associated objects.
/// </summary>
/// <typeparam name="T">The object type to attach to</typeparam>
public abstract class Behavior<T> : Behavior where T : Control
{
    /// <summary>
    /// Gets the object to which this behavior is attached.
    /// </summary>
    public new T? AssociatedObject => base.AssociatedObject as T;

    /// <summary>
    /// Called after the behavior is attached to the <see cref="Behavior.AssociatedObject"/>.
    /// </summary>
    /// <remarks>
    /// Override this to hook up functionality to the <see cref="Behavior.AssociatedObject"/>
    /// </remarks>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null && base.AssociatedObject is { })
        {
            string? actualType = base.AssociatedObject?.GetType().FullName;
            string? expectedType = typeof(T).FullName;
            string message = $"AssociatedObject is of type {actualType} but should be of type {expectedType}.";
            throw new InvalidOperationException(message);
        }
    }
}