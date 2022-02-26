#region

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Rendering;
using Avalonia.Styling;

#endregion

namespace PleasantUI.Controls.Custom
{
    [PseudoClasses(":open", ":close")]
    public class PleasantModalWindow : ContentControl, IStyleable, ICustomSimpleHitTest
    {
        public static readonly RoutedEvent WindowClosedEvent =
            RoutedEvent.Register<PleasantModalWindow, RoutedEventArgs>("WindowClosed", RoutingStrategies.Direct);

        public static readonly RoutedEvent WindowOpenedEvent =
            RoutedEvent.Register<PleasantModalWindow, RoutedEventArgs>("WindowOpened", RoutingStrategies.Direct);

        public static readonly DirectProperty<PleasantModalWindow, bool> IsClosedProperty =
            AvaloniaProperty.RegisterDirect<PleasantModalWindow, bool>(nameof(IsClosed), o => o.IsClosed,
                (o, v) => o.IsClosed = v);

        public static readonly DirectProperty<PleasantModalWindow, bool> IsClosingProperty =
            AvaloniaProperty.RegisterDirect<PleasantModalWindow, bool>(nameof(IsClosing), o => o.IsClosing);

        public static readonly StyledProperty<bool> MacOsModeProperty =
            AvaloniaProperty.Register<PleasantModalWindow, bool>(nameof(MacOsMode));

        private object _dialogResult;

        private PleasantWindow _host;

        private bool _isClosed;
        private bool _isClosing;

        static PleasantModalWindow()
        {
        }

        public PleasantModalWindow()
        {
            this.GetObservable(IsClosedProperty).Subscribe(x =>
            {
                if (!IsClosing && !IsClosed) return;

                RaiseEvent(new RoutedEventArgs(WindowClosedEvent));
            });
        }

        public bool IsClosed
        {
            get => _isClosed;
            set => SetAndRaise(IsClosedProperty, ref _isClosed, value);
        }

        public bool IsClosing
        {
            get => _isClosing;
            set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
        }

        public bool MacOsMode
        {
            get => GetValue(MacOsModeProperty);
            private set => SetValue(MacOsModeProperty, value);
        }

        public bool CanOpen { get; set; }

        public bool HitTest(Point point)
        {
            return VisualChildren.HitTestCustom(point);
        }

        Type IStyleable.StyleKey => typeof(ContentControl);

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler Opening;
        public event EventHandler Closing;

        protected virtual void OnOpened()
        {
            Opened?.Invoke(this, null);
        }

        protected virtual void OnOpening()
        {
            Opening?.Invoke(this, null);
        }

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, null);
        }

        protected virtual void OnClosing(CancelEventArgs args)
        {
            Closing?.Invoke(this, args);
        }

#pragma warning disable CS1998
        protected virtual async Task OnAnimation()
        {
        }
#pragma warning restore CS1998

        public Task Show(PleasantWindow host)
        {
            return Show<object>(host);
        }

        public Task<T> Show<T>(PleasantWindow host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));

            RaiseEvent(new RoutedEventArgs(WindowOpenedEvent));

            _host.AddModalWindow(this);

            TaskCompletionSource<T> result = new TaskCompletionSource<T>();

            Observable.FromEventPattern<EventHandler, EventArgs>(
                    x => Closed += x,
                    x => Closed -= x)
                .Take(1)
                .Subscribe(_ => { result.SetResult((T)(_dialogResult ?? default(T))); });

            OnOpened();

            CanOpen = true;

            return result.Task;
        }

        public void Close()
        {
            Close(false);
        }

        public void Close(object dialogResult)
        {
            _dialogResult = dialogResult;
            Close(false);
        }

        internal async void Close(bool ignoreCancel)
        {
            bool close = true;

            try
            {
                if (!ignoreCancel && ShouldCancelClose()) close = false;
            }
            finally
            {
                if (close)
                {
                    RaiseEvent(new RoutedEventArgs(WindowClosedEvent));
                    OnClosed();
                    PseudoClasses.Set(":close", true);

                    await OnAnimation();

                    CanOpen = false;

                    _host.RemoveModalWindow(this);
                }
            }
        }

        private bool ShouldCancelClose(CancelEventArgs args = null)
        {
            if (args is null) args = new CancelEventArgs();

            bool canClose = true;

            if (canClose)
            {
                OnClosing(args);
                return args.Cancel;
            }

            return true;
        }
    }
}