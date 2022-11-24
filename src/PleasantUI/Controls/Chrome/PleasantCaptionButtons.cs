using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PleasantUI.Enums;

namespace PleasantUI.Controls;

[PseudoClasses(":minimized", ":normal", ":maximized", ":isactive")]
public class PleasantCaptionButtons : TemplatedControl
{
    private CompositeDisposable? _disposable;

    private Button? _maximizeButton;
    private Button? _minimizeButton;

    public PleasantWindow? Host;
    
    public static readonly StyledProperty<bool> IsMacOsProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(IsMacOs));
    
    public bool IsMacOs
    {
        get => GetValue(IsMacOsProperty);
        set => SetValue(IsMacOsProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        e.NameScope.Get<Button>("PART_CloseButton").Click += (_, _) => Host?.Close();

        _maximizeButton = e.NameScope.Get<Button>("PART_MaximizeButton");
        _minimizeButton = e.NameScope.Get<Button>("PART_MinimizeButton");

        _maximizeButton.Click += (_, _) =>
        {
            if (Host is null) return;

            Host.WindowState = Host.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        };

        _minimizeButton.Click += (_, _) =>
        {
            if (Host is null) return;

            Host.WindowState = WindowState.Minimized;
        };

        if (_disposable is null && Host is not null)
        {
            _disposable = new CompositeDisposable
            {
                Host.GetObservable(Window.WindowStateProperty).Subscribe(x =>
                {
                    PseudoClasses.Set(":minimized", x == WindowState.Minimized);
                    PseudoClasses.Set(":normal", x == WindowState.Normal);
                    PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                }),
                Host.GetObservable(Window.CanResizeProperty).Subscribe(x =>
                {
                    _maximizeButton.IsEnabled = x;
                }),
                Host.GetObservable(Window.IsActiveProperty).Subscribe(x =>
                {
                    PseudoClasses.Set(":isactive", !x);
                }),
                Host.GetObservable(PleasantWindow.TitleBarTypeProperty).Subscribe(bar =>
                {
                    switch (bar)
                    {
                        case TitleBarType.Classic:
                            IsMacOs = false;
                            break;
                        case TitleBarType.NavigationView:
                            IsMacOs = false;
                            break;
                        case TitleBarType.NavigationViewExtended:
                            IsMacOs = false;
                            break;
                        case TitleBarType.MacOs:
                            IsMacOs = true;
                            break;
                        case TitleBarType.ExtendedWithoutContent:
                            IsMacOs = false;
                            break;
                        case TitleBarType.ExtendedWithContent:
                            IsMacOs = false;
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bar), bar, null);
                    }
                }),
                Host.GetObservable(PleasantWindow.WindowButtonsProperty).Subscribe(x =>
                {
                    if (x != WindowButtons.All)
                    {
                        if (x != WindowButtons.CloseAndCollapse) _minimizeButton.IsVisible = false;
                        if (x != WindowButtons.CloseAndExpand) _maximizeButton.IsVisible = false;
                    }
                }),
            };
        }
    }

    public void Detach()
    {
        if (_disposable == null) return;
        
        _disposable.Dispose();
        _disposable = null;
        Host = null;
    }
}