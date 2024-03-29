﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PleasantUI.Xaml.Interactivity;

namespace PleasantUI.Xaml.Behaviors;

/// <summary>
/// A behavior that allows to hide control on key down event.
/// </summary>
public class HideOnKeyPressedBehavior : Behavior<Control>
{
    /// <summary>
    /// Identifies the <seealso cref="TargetControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control> TargetControlProperty =
        AvaloniaProperty.Register<HideOnKeyPressedBehavior, Control>(nameof(TargetControl));

    /// <summary>
    /// Identifies the <seealso cref="Key"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Key> KeyProperty =
        AvaloniaProperty.Register<HideOnKeyPressedBehavior, Key>(nameof(Key), Key.Escape);

    /// <summary>
    /// Gets or sets the target control. This is a avalonia property.
    /// </summary>
    public Control TargetControl
    {
        get => GetValue(TargetControlProperty);
        set => SetValue(TargetControlProperty, value);
    }

    /// <summary>
    /// Gets or sets the key. This is a avalonia property.
    /// </summary>
    public Key Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    /// <summary>
    /// Called after the behavior is attached to the <see cref="Behavior.AssociatedObject"/>.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, AssociatedObject_KeyDown, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Called when the behavior is being detached from its <see cref="Behavior.AssociatedObject"/>.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, AssociatedObject_KeyDown);
    }

    private void AssociatedObject_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key)
            TargetControl.IsVisible = false;
    }
}