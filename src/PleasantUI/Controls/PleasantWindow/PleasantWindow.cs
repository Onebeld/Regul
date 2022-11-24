using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PleasantUI.EventArgs;
using PleasantUI.Interfaces;

#if OSX
using PleasantUI.Enums;
#endif


namespace PleasantUI.Controls;

public partial class PleasantWindow : Window, IStyleable, IPleasantWindowModal
{
    Type IStyleable.StyleKey => typeof(PleasantWindow);
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindows = e.NameScope.Get<Panel>("PART_ModalWindow");
        
#if OSX
        TypeTitleBar = TypePleasantTitleBar.MacOsStyle;
#endif

        ExtendClientAreaToDecorationsHint = PleasantUiSettings.Instance.EnableCustomTitleBar;

        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
        // this.GetObservable(TitleBarMenuProperty).Subscribe(val => { ShowMenuBar = val is null == false; });
        this.GetObservable(TitleBarTypeProperty).Subscribe(titleBarType =>
        {
            TitleBarTypeChangedEventArgs eventArgs = new(TitleBarTypeChangedEvent, titleBarType);
            RaiseEvent(eventArgs);
        });
        this.GetObservable(CanResizeProperty).Subscribe(canResize =>
        {
            ExtendClientAreaTitleBarHeightHint = canResize ? 8 : 1;
        });
        _modalWindows.Children.CollectionChanged += ChildrenOnCollectionChanged;
    }
    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TitleBar?.ForceSetVisible(_modalWindows.Children.Count > 0);
    }

    public void AddModalWindow(ModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(new ModalBackground());
        windowPanel.Children.Add(modalWindow);
        
        _modalWindows.Children.Add(windowPanel);
    }

    public void RemoveModalWindow(ModalWindow modalWindow)
    {
        _modalWindows.Children.Remove(modalWindow.Parent!);
    }
}