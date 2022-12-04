using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Extensions;
using PleasantUI.Generators;

namespace PleasantUI.Controls;

[TemplatePart("PART_ScrollViewer", typeof(SmoothScrollViewer))]
[TemplatePart("PART_AdderButton", typeof(Button))]
public partial class PleasantTabView : TabControl
{
    static PleasantTabView()
    {
        SelectionModeProperty.OverrideDefaultValue<PleasantTabView>(SelectionMode.Single);
    }

    protected void AdderButtonClicked(object? sender, RoutedEventArgs e)
    {
        RoutedEventArgs routedEventArgs = new(ClickOnAddingButtonEvent);
        RaiseEvent(routedEventArgs);
        routedEventArgs.Handled = true;
    }

    /// <inheritdoc />
    protected override IItemContainerGenerator CreateItemContainerGenerator()
    {
        return new PleasantTabItemContainerGenerator(
            this,
            ContentControl.ContentProperty,
            ContentControl.ContentTemplateProperty);
    }


    protected override async void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty && SelectedItem == null)
        {
            await Task.Delay(100);

            double d = ItemCount * 0.5;
            if ((_lastSelectIndex < d) & (ItemCount != 0))
            {
                SelectedItem = (Items as IList)?.OfType<object>().FirstOrDefault();
            }
            else if ((_lastSelectIndex >= d) & (ItemCount != 0))
                SelectedItem = (Items as IList)?.OfType<object>().LastOrDefault();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        AdderButton = e.NameScope.Find<Button>("PART_AdderButton");

        if (AdderButton != null) AdderButton.Click += AdderButtonClicked;
        _grid = e.NameScope.Find<Grid>("PART_InternalGrid");

        PropertyChanged += PleasantTabView_PropertyChanged;
    }

    private void PleasantTabView_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_grid is null) return;

        WidthRemainingSpace = _grid.Bounds.Width;
        HeightRemainingSpace = _grid.Bounds.Height;
    }

    /// <summary>
    ///     Add a <see cref="PleasantTabItem" />
    /// </summary>
    /// <param name="itemToAdd">The Item to Add</param>
    /// <param name="isSelected"></param>
    public void AddTab(PleasantTabItem itemToAdd, bool isSelected = true)
    {
        TabViewExtensions.AddTab(this, itemToAdd, isSelected);
    }
}