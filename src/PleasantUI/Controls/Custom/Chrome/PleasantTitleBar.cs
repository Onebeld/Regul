#region

using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

#endregion

namespace PleasantUI.Controls.Custom.Chrome
{
    [PseudoClasses(":minimized", ":normal", ":maximized", ":fullscreen", ":isactive")]
    public class PleasantTitleBar : TemplatedControl
    {
        private PleasantCaptionButtons _captionButtons;

        private MenuItem _closeMenuItem;
        private MenuItem _collapseMenuItem;
        private ContextMenu _contextMenu;
        private CompositeDisposable _disposables;
        private Border _dragWindow;
        private MenuItem _expandMenuItem;
        private Image _image;
        private Path _logo;

        private ToggleButton _pinButton;
        private MenuItem _reestablishMenuItem;
        private Separator _separator;
        private TextBlock _title;
        private ContentPresenter _titleBarMenu;
        private StackPanel _titlePanel;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _captionButtons?.Detach();

            _captionButtons = e.NameScope.Get<PleasantCaptionButtons>("PART_CaptionButtons");
            _image = e.NameScope.Get<Image>("PART_Icon");
            _title = e.NameScope.Get<TextBlock>("PART_Title");
            _logo = e.NameScope.Get<Path>("PART_Logo");
            _titleBarMenu = e.NameScope.Get<ContentPresenter>("PART_TitleBarMenu");
            _dragWindow = e.NameScope.Get<Border>("PART_DragWindow");
            _contextMenu = e.NameScope.Get<ContextMenu>("PART_ContextMenu");
            _titlePanel = e.NameScope.Get<StackPanel>("PART_TitlePanel");

            _closeMenuItem = e.NameScope.Get<MenuItem>("PART_CloseMenuItem");
            _collapseMenuItem = e.NameScope.Get<MenuItem>("PART_CollapseMenuItem");
            _expandMenuItem = e.NameScope.Get<MenuItem>("PART_ExpandMenuItem");
            _reestablishMenuItem = e.NameScope.Get<MenuItem>("PART_ReestablishMenuItem");
            _separator = e.NameScope.Get<Separator>("PART_Separator");

            _pinButton = e.NameScope.Get<ToggleButton>("PART_PinButton");

            if (VisualRoot is PleasantWindow window)
            {
                _captionButtons.HostWindow = window;

                _closeMenuItem.Click += (_, e1) => window.Close();
                _reestablishMenuItem.Click += (_, e1) => window.WindowState = WindowState.Normal;
                _expandMenuItem.Click += (_, e1) => window.WindowState = WindowState.Maximized;
                _collapseMenuItem.Click += (_, e1) => window.WindowState = WindowState.Minimized;

                _pinButton.Click += (_, e1) =>
                    window.Topmost = _pinButton.IsChecked is null == false && (bool)_pinButton.IsChecked;

                _dragWindow.PointerPressed += (_, e1) =>
                {
                    if (window.WindowState != WindowState.FullScreen)
                    {
                        _contextMenu.Close();
                        window.PlatformImpl?.BeginMoveDrag(e1);
                    }
                };
                _dragWindow.DoubleTapped += (_, e1) =>
                {
                    if (window.WindowButtons == WindowButtons.CloseAndExpand ||
                        window.WindowButtons == WindowButtons.All)
                        window.WindowState = window.WindowState == WindowState.Maximized
                            ? WindowState.Normal
                            : WindowState.Maximized;
                };
                window.PointerReleased += (_, e1) => _contextMenu.Close();
            }

            Attach();
        }

        private void Attach()
        {
            if (VisualRoot is PleasantWindow window)
                _disposables = new CompositeDisposable
                {
                    window.GetObservable(Window.WindowStateProperty)
                        .Subscribe(x =>
                        {
                            PseudoClasses.Set(":minimized", x == WindowState.Minimized);
                            PseudoClasses.Set(":normal", x == WindowState.Normal);
                            PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                            PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);

                            if (x == WindowState.Maximized)
                            {
                                _reestablishMenuItem.IsEnabled = true;
                                _expandMenuItem.IsEnabled = false;
                            }
                            else
                            {
                                _reestablishMenuItem.IsEnabled = false;
                                _expandMenuItem.IsEnabled = true;
                            }

                            if (x == WindowState.FullScreen)
                            {
                                _reestablishMenuItem.IsEnabled = false;
                                _expandMenuItem.IsEnabled = false;
                                _collapseMenuItem.IsEnabled = false;
                            }
                            else
                            {
                                _collapseMenuItem.IsEnabled = true;
                            }
                        }),
                    window.GetObservable(WindowBase.IsActiveProperty)
                        .Subscribe(x => { PseudoClasses.Set(":isactive", !x); }),
                    window.GetObservable(Window.IconProperty)
                        .Subscribe(x => _image.Source = window.Icon.ToBitmap()),
                    window.GetObservable(Window.TitleProperty)
                        .Subscribe(x => { _title.Text = x; }),
                    window.GetObservable(PleasantWindow.LogoProperty)
                        .Subscribe(x =>
                        {
                            _logo.Data = x;
                            _logo.IsVisible = _logo.Data is null == false;
                            _title.IsVisible = _logo.Data is null;
                        }),
                    window.GetObservable(PleasantWindow.TitleBarMenuProperty)
                        .Subscribe(x => { _titleBarMenu.Content = x; }),
                    window.GetObservable(PleasantWindow.WindowButtonsProperty)
                        .Subscribe(x =>
                        {
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
                                default:
                                    _expandMenuItem.IsVisible = false;
                                    _reestablishMenuItem.IsVisible = false;
                                    _collapseMenuItem.IsVisible = false;
                                    _separator.IsVisible = false;
                                    break;
                            }
                        }),
                    window.GetObservable(PleasantWindow.ShowPinButtonProperty).Subscribe(x =>
                    {
                        _pinButton.IsVisible = x;
                    }),
                    window.GetObservable(PleasantWindow.ShowCustomTitleBarProperty).Subscribe(x =>
                    {
                        if (x)
                        {
                            IsVisible = true;

                            _titlePanel.IsVisible = true;
                            _dragWindow.IsVisible = true;
                            _titleBarMenu.Margin = new Thickness(5, 0);
                            _captionButtons.IsVisible = true;
                        }
                        else
                        {
                            if (window.WindowState == WindowState.FullScreen)
                                window.WindowState = WindowState.Normal;

                            _titlePanel.IsVisible = false;
                            _dragWindow.IsVisible = false;
                            _titleBarMenu.Margin = new Thickness(1, 0);
                            _captionButtons.IsVisible = false;

                            if (window.TitleBarMenu is null || window.ShowMenuBar == false) IsVisible = false;
                        }
                    })
                };
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _disposables?.Dispose();

            _captionButtons?.Detach();
            _captionButtons = null;
        }
    }
}