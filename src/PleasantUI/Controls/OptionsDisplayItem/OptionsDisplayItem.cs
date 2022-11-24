using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

public partial class OptionsDisplayItem : TemplatedControl
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NavigatesProperty)
        {
            if (Expands)
                throw new InvalidOperationException("Control cannot both Navigate and Expand");

            PseudoClasses.Set(":navigates", (bool?)change.NewValue ?? false);
        }
        else if (change.Property == ExpandsProperty)
        {
            if (Navigates)
                throw new InvalidOperationException("Control cannot both Navigate and Expand");

            PseudoClasses.Set(":expands", (bool?)change.NewValue ?? false);
        }
        else if (change.Property == IsExpandedProperty)
        {
            PseudoClasses.Set(":expanded", (bool?)change.NewValue ?? false);
        }
        else if (change.Property == IconProperty)
        {
            PseudoClasses.Set(":icon", change.NewValue != null);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        _layoutRoot = e.NameScope.Find<Border>("LayoutRoot")!;
        _layoutRoot.PointerPressed += OnLayoutRootPointerPressed;
        _layoutRoot.PointerReleased += OnLayoutRootPointerReleased;
        _layoutRoot.PointerCaptureLost += OnLayoutRootPointerCaptureLost;
    }
    
    private void OnLayoutRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            _isPressed = true;
            PseudoClasses.Set(":pressed", true);
        }
    }

    private void OnLayoutRootPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        PointerPoint pt = e.GetCurrentPoint(this);
        if (_isPressed && pt.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            _isPressed = false;

            PseudoClasses.Set(":pressed", false);

            if (Expands)
                IsExpanded = !IsExpanded;

            if (Navigates)
            {
                RaiseEvent(new RoutedEventArgs(NavigationRequestedEvent, this));
                NavigationCommand?.Execute(null);                                        
            }
        }
    }

    private void OnLayoutRootPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isPressed = false;
        PseudoClasses.Set(":pressed", false);
    }
}