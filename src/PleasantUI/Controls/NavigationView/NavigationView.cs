using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

[PseudoClasses(":normal", ":compact")]
public sealed partial class NavigationView : TreeView, IContentPresenterHost, IHeadered
{
    private Button? _headerItem;
    private Button? _backButton;
    private SplitView? _splitView;
    private const double LittleWidth = 1005;
    private const double VeryLittleWidth = 650;

    static NavigationView()
    {
        SelectionModeProperty.OverrideDefaultValue<NavigationView>(SelectionMode.Single);
        SelectedItemProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnSelectedItemChanged());
        FocusableProperty.OverrideDefaultValue<NavigationView>(true);
        IsOpenProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnIsOpenChanged());
        IsFloatingHeaderProperty.Changed.Subscribe(x =>
        {
            if (x.Sender is NavigationView navigationView)
                navigationView.UpdateHeaderVisibility();
        });
    }

    public NavigationView()
    {
        PseudoClasses.Add(":normal");
        this.GetObservable(BoundsProperty).Subscribe(bounds =>
        {
            Dispatcher.UIThread.InvokeAsync(() => OnBoundsChanged(bounds));
        });
    }

    private void OnBoundsChanged(Rect rect)
    {
        if (DynamicDisplayMode)
        {
            bool isLittle = rect.Width <= LittleWidth;
            bool isVeryLittle = rect.Width <= VeryLittleWidth;

            if (!isLittle && !isVeryLittle)
            {
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            else if (isLittle && !isVeryLittle)
            {
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactOverlay;
                IsOpen = false;
                foreach (NavigationViewItemBase navigationViewItemBase in this.GetLogicalDescendants().OfType<NavigationViewItemBase>())
                {
                    navigationViewItemBase.IsExpanded = false;
                }
            }
            else if (isLittle && isVeryLittle)
            {
                UpdatePseudoClasses(true);
                DisplayMode = SplitViewDisplayMode.Overlay;
                IsOpen = false;
                foreach (NavigationViewItemBase navigationViewItemBase in this.GetLogicalDescendants().OfType<NavigationViewItemBase>())
                {
                    navigationViewItemBase.IsExpanded = false;
                }
            }
        }
    }

    internal void SelectSingleItemCore(object? item)
    {
        if (SelectedItem != item)
        {
            PseudoClasses.Remove(":normal");
            PseudoClasses.Add(":normal");
        }

        if (SelectedItem is ISelectable selectableSelectedItem)
            selectableSelectedItem.IsSelected = false;

        if (item is ISelectable selectableItem)
            selectableItem.IsSelected = true;

        SelectedItems.Clear();
        SelectedItems.Add(item);

        SelectedItem = item;
    }

    internal void SelectSingleItem(object item)
    {
        SelectSingleItemCore(item);
    }

    private void UpdateHeaderVisibility() => HeaderVisible = IsOpen | IsFloatingHeader;

    private void OnSelectedItemChanged()
    {
        UpdateTitleAndSelectedContent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _headerItem = e.NameScope.Find<Button>("PART_HeaderItem");
        _splitView = e.NameScope.Find<SplitView>("split");
        _backButton = e.NameScope.Find<Button>("PART_BackButton");

        if (_headerItem != null)
            _headerItem.Click += delegate
            {
                if (!AlwaysOpen)
                    IsOpen = !IsOpen;
                else
                    IsOpen = true;
            };

        BackButtonCommandProperty.Changed.Subscribe(x =>
        {
            if (_backButton is not null)
                _backButton.IsVisible = x.NewValue.Value is not null;
        });

        UpdateTitleAndSelectedContent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        if (Items is IList { Count: >= 1 } l && l[0] is ISelectable s)
            SelectSingleItem(s);
    }

    ///<inheritdoc/>
    IAvaloniaList<ILogical> IContentPresenterHost.LogicalChildren => LogicalChildren;

    bool IContentPresenterHost.RegisterContentPresenter(IContentPresenter presenter)
    {
        return RegisterContentPresenter(presenter);
    }

    private bool RegisterContentPresenter(IContentPresenter presenter) =>
        presenter.Name == "PART_SelectedContentPresenter";

    ///<inheritdoc/>
    protected override void OnContainersMaterialized(ItemContainerEventArgs e)
    {
        base.OnContainersMaterialized(e);
        UpdateTitleAndSelectedContent();
    }

    ///<inheritdoc/>
    protected override void OnContainersDematerialized(ItemContainerEventArgs e)
    {
        base.OnContainersDematerialized(e);
        UpdateTitleAndSelectedContent();
    }

    private void OnIsOpenChanged()
    {
        UpdateHeaderVisibility();
    }

    private void UpdatePseudoClasses(bool isCompact)
    {
        switch (isCompact)
        {
            case true:
                PseudoClasses.Add(":compact");
                break;
            case false:
                PseudoClasses.Remove(":compact");
                break;
        }
    }

    private void UpdateTitleAndSelectedContent()
    {
        if (SelectedItem is NavigationViewItemBase { TypeContent: { } } itemBase)
            SelectedContent = Activator.CreateInstance(itemBase.TypeContent);
    }
}