using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PleasantUI.Controls.Custom;

namespace PleasantUI.Windows;

public class WindowColorPicker : PleasantDialogWindow
{
    public WindowColorPicker()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static Task<Color> SelectColor(PleasantWindow parent, string? defaultColor = null)
    {
        WindowColorPicker windowColorPicker = new()
        {
            Icon = parent.Icon.ToBitmap()
        };

        ColorPicker picker = windowColorPicker.FindControl<ColorPicker>("ColorPicker");
        bool cancel = true;
        Color res = defaultColor is null ? new Color(255, 255, 255, 255) : Color.Parse(defaultColor);

        picker.Color = res;
        picker.ChangeColor += (_, _) => { res = picker.Color; };

        windowColorPicker.FindControl<Button>("Cancel").Click += (_, _) => { windowColorPicker.Close(); };
        windowColorPicker.FindControl<Button>("OK").Click += (_, _) =>
        {
            cancel = false;
            windowColorPicker.Close();
        };

        windowColorPicker.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                cancel = false;
                windowColorPicker.Close();
            }
        };

        TaskCompletionSource<Color> tcs = new();
        windowColorPicker.Closed += (_, _) =>
        {
            if (!cancel) tcs.TrySetResult(res);
            else tcs.TrySetCanceled();
        };
        windowColorPicker.Show(parent);
        return tcs.Task;
    }
}