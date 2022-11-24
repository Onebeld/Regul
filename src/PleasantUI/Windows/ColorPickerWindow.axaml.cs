using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Interfaces;

namespace PleasantUI.Windows;

public class ColorPickerWindow : ContentDialog
{
    public ColorPickerWindow() => AvaloniaXamlLoader.Load(this);

    public static Task<Color?> SelectColor(IPleasantWindowModal parent, uint? defaultColor = null)
    {
        bool cancel = true;
        ColorPickerWindow window = new();

        ColorView colorView = window.FindControl<ColorView>("ColorView")!;
        colorView.Color = Color.FromUInt32(defaultColor ?? 0xFFFFFFFF);

        window.FindControl<Button>("CancelButton")!.Click += (_, _) => { window.Close(); };
        window.FindControl<Button>("OkButton")!.Click += (_, _) =>
        {
            cancel = false;
            window.Close();
        };
        window.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            
            cancel = false;
            window.Close();
        };

        TaskCompletionSource<Color?> taskCompletionSource = new();

        window.Closed += (_, _) =>
        {
            taskCompletionSource.TrySetResult(!cancel ? colorView.Color : null);
        };
        window.Show(parent);

        return taskCompletionSource.Task;
    }
}