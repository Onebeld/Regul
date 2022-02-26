#region

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PleasantUI.Controls.Custom;

#endregion

namespace PleasantUI.Windows
{
    public class WindowColorPicker : PleasantDialogWindow
    {
        public WindowColorPicker()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<Color> SelectColor(PleasantWindow parent, string defaultColor = null)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            WindowColorPicker windowColorPicker = new WindowColorPicker
            {
                Icon = parent.Icon.ToBitmap()
            };

            ColorPicker picker = windowColorPicker.FindControl<ColorPicker>("ColorPicker");
            bool cancel = true;
            Color res = Color.Parse(defaultColor);

            picker.Color = res;
            picker.ChangeColor += (s, e) => { res = picker.Color; };

            windowColorPicker.FindControl<Button>("Cancel").Click += (s, e) => { windowColorPicker.Close(); };
            windowColorPicker.FindControl<Button>("OK").Click += (s, e) =>
            {
                cancel = false;
                windowColorPicker.Close();
            };

            windowColorPicker.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cancel = false;
                    windowColorPicker.Close();
                }
            };

            TaskCompletionSource<Color> tcs = new TaskCompletionSource<Color>();
            windowColorPicker.Closed += (s, e) =>
            {
                if (!cancel) tcs.TrySetResult(res);
                else tcs.TrySetCanceled();
            };
            windowColorPicker.Show(parent);
            return tcs.Task;
        }
    }
}