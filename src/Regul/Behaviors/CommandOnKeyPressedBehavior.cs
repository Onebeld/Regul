using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PleasantUI.Xaml.Interactivity;

namespace Regul.Behaviors;

public class CommandOnKeyPressedBehavior : Behavior<Control>
{
    /// <summary>
    /// Defines the <see cref="Command"/> property.
    /// </summary>
    public static readonly DirectProperty<CommandOnKeyPressedBehavior, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<CommandOnKeyPressedBehavior, ICommand?>(nameof(Command), behavior => behavior.Command, (behavior, command) => behavior.Command = command, enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty =
        AvaloniaProperty.Register<CommandOnKeyPressedBehavior, object>(nameof(CommandParameter));

    /// <summary>
    /// Identifies the <seealso cref="Key"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Key> KeyProperty =
        AvaloniaProperty.Register<CommandOnKeyPressedBehavior, Key>(nameof(Key), Key.Escape);

    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<CommandOnKeyPressedBehavior, bool>(nameof(IsEnabled), true);

    private ICommand? _command;

    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the key is downed.
    /// </summary>
    public ICommand? Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
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
    /// Gets or sets a parameter to be passed to the <see cref="Command"/>.
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool IsEnabled
    {
        get => GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, AssociatedObject_KeyDown, RoutingStrategies.Bubble);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, AssociatedObject_KeyDown);
    }

    private void AssociatedObject_KeyDown(object? sender, KeyEventArgs e)
    {
        if (IsEnabled && e.Key == Key) 
            Command?.Execute(CommandParameter);
    }
}