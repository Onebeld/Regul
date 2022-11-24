using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PleasantUI.Xaml.Interactivity;

namespace Regul.Behaviors;

public class CommandOnDoubleTappedBehavior : Behavior<Control>
{
    /// <summary>
    /// Defines the <see cref="Command"/> property.
    /// </summary>
    public static readonly DirectProperty<CommandOnDoubleTappedBehavior, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<CommandOnDoubleTappedBehavior, ICommand?>(nameof(Command), behavior => behavior.Command, (behavior, command) => behavior.Command = command, enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty =
        AvaloniaProperty.Register<CommandOnDoubleTappedBehavior, object>(nameof(CommandParameter));


    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<CommandOnDoubleTappedBehavior, bool>(nameof(IsEnabled), true);

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
        AssociatedObject?.AddHandler(InputElement.DoubleTappedEvent, AssociatedObject_DoubleTapped, RoutingStrategies.Bubble);
    }
    
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(InputElement.DoubleTappedEvent, AssociatedObject_DoubleTapped);
    }
    
    private void AssociatedObject_DoubleTapped(object? sender, TappedEventArgs e)
    {
        Command?.Execute(CommandParameter);
    }
}