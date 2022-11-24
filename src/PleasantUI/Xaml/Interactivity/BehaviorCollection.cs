using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;

namespace PleasantUI.Xaml.Interactivity;

/// <summary>
/// Represents a collection of <see cref="IBehavior"/>'s with a shared <see cref="AssociatedObject"/>.
/// </summary>
public class BehaviorCollection : AvaloniaList<IAvaloniaObject>
{
    // After a VectorChanged event we need to compare the current state of the collection
    // with the old collection so that we can call Detach on all removed items.
    private readonly List<IBehavior> _oldCollection = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BehaviorCollection"/> class.
    /// </summary>
    public BehaviorCollection()
    {
        CollectionChanged += BehaviorCollection_CollectionChanged;
    }

    /// <summary>
    /// Gets the <see cref="IAvaloniaObject"/> to which the <see cref="BehaviorCollection"/> is attached.
    /// </summary>
    public IAvaloniaObject? AssociatedObject
    {
        get;
        private set;
    }

    /// <summary>
    /// Attaches the collection of behaviors to the specified <see cref="IAvaloniaObject"/>.
    /// </summary>
    /// <param name="associatedObject">The <see cref="IAvaloniaObject"/> to which to attach.</param>
    /// <exception cref="InvalidOperationException">The <see cref="BehaviorCollection"/> is already attached to a different <see cref="IAvaloniaObject"/>.</exception>
    public void Attach(IAvaloniaObject? associatedObject)
    {
        if (Equals(associatedObject, AssociatedObject))
        {
            return;
        }

        if (AssociatedObject is { })
        {
            throw new InvalidOperationException(
                "An instance of a behavior cannot be attached to more than one object at a time.");
        }
        
        AssociatedObject = associatedObject;

        foreach (IAvaloniaObject? item in this)
        {
            IBehavior behavior = (IBehavior)item;
            behavior.Attach(AssociatedObject);
        }
    }

    /// <summary>
    /// Detaches the collection of behaviors from the <see cref="BehaviorCollection.AssociatedObject"/>.
    /// </summary>
    public void Detach()
    {
        foreach (IAvaloniaObject? item in this)
        {
            if (item is IBehavior { AssociatedObject: { } } behaviorItem)
            {
                behaviorItem.Detach();
            }
        }

        AssociatedObject = null;
        _oldCollection.Clear();
    }

    internal void AttachedToVisualTree()
    {
        foreach (IAvaloniaObject? item in this)
        {
            if (item is Behavior behavior)
            {
                behavior.AttachedToVisualTree();
            }
        }
    }

    internal void DetachedFromVisualTree()
    {
        foreach (IAvaloniaObject? item in this)
        {
            if (item is Behavior { AssociatedObject: { } } behavior)
            {
                behavior.DetachedFromVisualTree();
            }
        }
    }

    private void BehaviorCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (IBehavior behavior in _oldCollection)
            {
                if (behavior.AssociatedObject is { })
                {
                    behavior.Detach();
                }
            }

            _oldCollection.Clear();

            foreach (IAvaloniaObject? newItem in this)
            {
                _oldCollection.Add(VerifiedAttach(newItem));
            }
            return;
        }

        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                int eventIndex = eventArgs.NewStartingIndex;
                IAvaloniaObject? changedItem = eventArgs.NewItems?[0] as IAvaloniaObject;
                _oldCollection.Insert(eventIndex, VerifiedAttach(changedItem));
            }
                break;

            case NotifyCollectionChangedAction.Replace:
            {
                int eventIndex = eventArgs.OldStartingIndex;
                eventIndex = eventIndex == -1 ? 0 : eventIndex;

                IAvaloniaObject? changedItem = eventArgs.NewItems?[0] as IAvaloniaObject;

                IBehavior oldItem = _oldCollection[eventIndex];
                if (oldItem.AssociatedObject is { })
                {
                    oldItem.Detach();
                }

                _oldCollection[eventIndex] = VerifiedAttach(changedItem);
            }
                break;

            case NotifyCollectionChangedAction.Remove:
            {
                int eventIndex = eventArgs.OldStartingIndex;

                IBehavior oldItem = _oldCollection[eventIndex];
                if (oldItem.AssociatedObject is { })
                {
                    oldItem.Detach();
                }

                _oldCollection.RemoveAt(eventIndex);
            }
                break;

            default:
                break;
        }
    }

    private IBehavior VerifiedAttach(IAvaloniaObject? item)
    {
        if (!(item is IBehavior behavior))
        {
            throw new InvalidOperationException(
                $"Only {nameof(IBehavior)} types are supported in a {nameof(BehaviorCollection)}.");
        }

        if (_oldCollection.Contains(behavior))
        {
            throw new InvalidOperationException(
                $"Cannot add an instance of a behavior to a {nameof(BehaviorCollection)} more than once.");
        }

        if (AssociatedObject is { })
        {
            behavior.Attach(AssociatedObject);
        }

        return behavior;
    }
}