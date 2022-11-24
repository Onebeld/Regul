using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Enums;
using PleasantUI.Extensions;
using Path = Avalonia.Controls.Shapes.Path;

namespace PleasantUI.Controls;

[PseudoClasses(":minimized", ":normal", ":maximized", ":isactive")]
public partial class PleasantTitleBar : TemplatedControl
{
    private bool _forcedSetIsVisible;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        _captionButtons?.Detach();

        _captionButtons = e.NameScope.Get<PleasantCaptionButtons>("PART_CaptionButtons");

        _closeMenuItem = e.NameScope.Get<MenuItem>("PART_CloseMenuItem");
        _expandMenuItem = e.NameScope.Get<MenuItem>("PART_ExpandMenuItem");
        _collapseMenuItem = e.NameScope.Get<MenuItem>("PART_CollapseMenuItem");
        _reestablishMenuItem = e.NameScope.Get<MenuItem>("PART_ReestablishMenuItem");
        _separator = e.NameScope.Get<Separator>("PART_SeparatorMenuItem");

        _image = e.NameScope.Get<Image>("PART_Icon");
        _title = e.NameScope.Get<TextBlock>("PART_Title");
        _description = e.NameScope.Get<TextBlock>("PART_Description");
        _logoPath = e.NameScope.Get<Path>("PART_LogoPath");
        _dragWindowBorder = e.NameScope.Get<Border>("PART_DragWindow");
        _titlePanel = e.NameScope.Get<StackPanel>("PART_TitlePanel");

        if (VisualRoot is PleasantWindow window)
        {
            _host = window;
            _captionButtons.Host = window;

            _closeMenuItem.Click += (_, _) => window.Close();
            _reestablishMenuItem.Click += (_, _) => window.WindowState = WindowState.Normal;
            _expandMenuItem.Click += (_, _) => window.WindowState = WindowState.Maximized;
            _collapseMenuItem.Click += (_, _) => window.WindowState = WindowState.Minimized;

            _dragWindowBorder.PointerPressed += OnDragWindowBorderOnPointerPressed;
            _dragWindowBorder.DoubleTapped += OnDragWindowBorderOnDoubleTapped;
            
            Attach(window);

            window.TitleBar = this;
        }
    }
    
    internal void OnDragWindowBorderOnPointerPressed(object? _, PointerPressedEventArgs args)
    {
        _host?.PlatformImpl?.BeginMoveDrag(args);
    }
    internal void OnDragWindowBorderOnDoubleTapped(object? o, TappedEventArgs tappedEventArgs)
    {
        if (_host is null || !_host.CanResize) return;
        _host.WindowState = _host.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    internal FlyoutBase? GetContextFlyout() => _dragWindowBorder?.ContextFlyout;

    private void Attach(PleasantWindow host)
    {
        _disposable = new CompositeDisposable
        {
            host.GetObservable(Window.WindowStateProperty).Subscribe(windowState =>
            {
                PseudoClasses.Set(":minimized", windowState == WindowState.Minimized);
                PseudoClasses.Set(":normal", windowState == WindowState.Normal);
                PseudoClasses.Set(":maximized", windowState == WindowState.Maximized);

                if (windowState == WindowState.Maximized)
                {
                    if (_reestablishMenuItem is not null) _reestablishMenuItem.IsEnabled = true;
                    if (_expandMenuItem is not null) _expandMenuItem.IsEnabled = false;
                }
                else
                {
                    if (_reestablishMenuItem is not null) _reestablishMenuItem.IsEnabled = false;
                    if (_expandMenuItem is not null) _expandMenuItem.IsEnabled = true;
                }
            }),
            host.GetObservable(WindowBase.IsActiveProperty).Subscribe(b =>
            {
                PseudoClasses.Set(":isactive", !b);
            }),
            host.GetObservable(Window.TitleProperty).Subscribe(s =>
            {
                if (_title is not null) _title.Text = s;
            }),
            host.GetObservable(PleasantWindow.DescriptionProperty).Subscribe(s =>
            {
                if (_description is not null) _description.Text = s;
            }),
            host.GetObservable(PleasantWindow.LogoGeometryProperty).Subscribe(geometry =>
            {
                if (_logoPath is not null)
                {
                    _logoPath.Data = geometry!;
                    _logoPath.IsVisible = geometry is null == false;
                }

                if (_title != null) _title.IsVisible = geometry is null;
            }),
            host.GetObservable(PleasantWindow.ImageIconProperty).Subscribe(image =>
            {
                if (image is not null)
                {
                    if (_image is not null) _image.Source = image;
                }
                else
                {
                    if (host.Icon is not null && _image is not null) 
                        _image.Source = host.Icon.ToBitmap();
                }
            }),
            host.GetObservable(Window.IconProperty).Subscribe(_ =>
            {
                if (host.ImageIcon is null)
                {
                    if (_image is not null && host.Icon is not null) 
                        _image.Source = host.Icon.ToBitmap();
                }
            }),
            host.GetObservable(PleasantWindow.EnableCustomTitleBarProperty).Subscribe(b =>
            {
                IsVisible = b;
            }),
            host.GetObservable(PleasantWindow.TitleBarTypeProperty).Subscribe(bar =>
            {
                switch (bar)
                {
                    case TitleBarType.Classic:
                        Height = 32;
                        Margin = Thickness.Parse("0");
                        if (_titlePanel != null) _titlePanel.Margin = Thickness.Parse("15 0 0 0");
                        break;
                    case TitleBarType.ExtendedWithContent:
                        Height = 44;
                        Margin = Thickness.Parse("0");
                        if (_titlePanel != null) _titlePanel.Margin = Thickness.Parse("15 0 0 0");
                        break;
                    case TitleBarType.ExtendedWithoutContent:
                        Height = 44;
                        Margin = Thickness.Parse("0");
                        break;
                    case TitleBarType.NavigationView:
                        Height = 44;
                        Margin = Thickness.Parse("50 0 0 0");
                        if (_titlePanel != null) _titlePanel.Margin = Thickness.Parse("2 0 0 0");
                        break;
                    case TitleBarType.NavigationViewExtended:
                        Height = 44;
                        Margin = Thickness.Parse("100 0 0 0");
                        if (_titlePanel != null) _titlePanel.Margin = Thickness.Parse("2 0 0 0");
                        break;
                    case TitleBarType.MacOs:
                        Height = 22;
                        Margin = Thickness.Parse("0");
                        if (_titlePanel != null) _titlePanel.Margin = Thickness.Parse("0");
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bar), bar, null);
                }

                IsMacOs = bar == TitleBarType.MacOs;
                if (_titlePanel != null) _titlePanel.IsVisible = bar != TitleBarType.ExtendedWithoutContent;
                if (_dragWindowBorder != null && !_forcedSetIsVisible)
                {
                    _dragWindowBorder.IsVisible = bar != TitleBarType.ExtendedWithoutContent;
                    _dragWindowBorder.IsHitTestVisible = bar != TitleBarType.ExtendedWithoutContent;
                }
            }),
            host.GetObservable(PleasantWindow.WindowButtonsProperty)
                .Subscribe(x =>
                {
                    if (_expandMenuItem is null || _reestablishMenuItem is null || _collapseMenuItem is null || _separator is null)
                        return;
                    
                    switch (x)
                    {
                        case WindowButtons.All:
                            _expandMenuItem.IsVisible = true;
                            _reestablishMenuItem.IsVisible = true;
                            _collapseMenuItem.IsVisible = true;
                            _separator.IsVisible = true;
                            break;
                        case WindowButtons.CloseAndCollapse:
                            _expandMenuItem.IsVisible = false;
                            _reestablishMenuItem.IsVisible = false;
                            _collapseMenuItem.IsVisible = true;
                            _separator.IsVisible = true;
                            break;
                        case WindowButtons.CloseAndExpand:
                            _expandMenuItem.IsVisible = true;
                            _reestablishMenuItem.IsVisible = true;
                            _collapseMenuItem.IsVisible = false;
                            _separator.IsVisible = true;
                            break;
                        
                        case WindowButtons.Close:
                        default:
                            _expandMenuItem.IsVisible = false;
                            _reestablishMenuItem.IsVisible = false;
                            _collapseMenuItem.IsVisible = false;
                            _separator.IsVisible = false;
                            break;
                    }
                })
        };
    }

    internal void ForceSetVisible(bool isVisible)
    {
        if (_dragWindowBorder is null || _host is null) return;

        if (isVisible)
        {
            _forcedSetIsVisible = true;

            _dragWindowBorder.IsVisible = true;
            _dragWindowBorder.IsHitTestVisible = true;
        }
        else
        {
            _forcedSetIsVisible = false;
            
            _dragWindowBorder.IsVisible = _host.TitleBarType != TitleBarType.ExtendedWithoutContent;
            _dragWindowBorder.IsHitTestVisible = _host.TitleBarType != TitleBarType.ExtendedWithoutContent;
        }
    }
}