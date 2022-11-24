using Avalonia;
using Avalonia.Metadata;

namespace PleasantUI.Xaml.Interactivity;

/// <summary>
/// A base class for behaviors, implementing the basic plumbing of <seealso cref="ITrigger"/>.
/// </summary>
public abstract class Trigger : Behavior, ITrigger
{
    /// <summary>
    /// Identifies the <seealso cref="Actions"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<Trigger, ActionCollection> ActionsProperty =
        AvaloniaProperty.RegisterDirect<Trigger, ActionCollection>(nameof(Actions), t => t.Actions);

    private ActionCollection? _actions;

    /// <summary>
    /// Gets the collection of actions associated with the behavior. This is a avalonia property.
    /// </summary>
    [Content]
    public ActionCollection Actions => _actions ??= new ActionCollection();
}

/// <summary>
/// A base class for behaviors, implementing the basic plumbing of <seealso cref="ITrigger"/>.
/// </summary>
/// <typeparam name="T">The object type to attach to</typeparam>
public abstract class Trigger<T> : Trigger where T : class, IAvaloniaObject
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