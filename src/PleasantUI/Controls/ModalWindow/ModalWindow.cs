using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Rendering;
using Avalonia.Styling;
using PleasantUI.Interfaces;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

[PseudoClasses(":close")]
public partial class ModalWindow : ContentControl, IStyleable, ICustomHitTest
{
    static ModalWindow() { }

    public bool HitTest(Point point) => VisualChildren.HitTestCustom(point);

    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, null!);
    }

#pragma warning disable CS1998
    protected virtual async Task OnAnimation()
    {
    }
#pragma warning restore CS1998

    public Task Show(IPleasantWindowModal host) => Show<object>(host);

    public Task<T?> Show<T>(IPleasantWindowModal host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));

        RaiseEvent(new RoutedEventArgs(WindowOpenedEvent));

        _host.AddModalWindow(this);

        TaskCompletionSource<T?> result = new();

        Observable.FromEventPattern(
                x => Closed += x,
                x => Closed -= x)
            .Take(1)
            .Subscribe(_ => { result.SetResult((T?)_dialogResult); });

        CanOpen = true;

        return result.Task;
    }

    Type IStyleable.StyleKey => typeof(Control);

    public void Close()
    {
        Close(false);
    }

    public void Close(object? dialogResult)
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
                ((ModalBackground)(Parent as Panel)!.Children[0]).Close();

                await OnAnimation();

                CanOpen = false;

                _host.RemoveModalWindow(this);
            }
        }
    }

    private bool ShouldCancelClose(CancelEventArgs? args = null)
    {
        args ??= new CancelEventArgs();
        return args.Cancel;
    }
}