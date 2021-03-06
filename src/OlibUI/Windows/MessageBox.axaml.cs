using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using OlibUI.Structures;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OlibUI.Windows
{
    public class MessageBox : OlibWindow
    {
        public enum MessageBoxIcon
        {
            Information,
            Error,
            Warning,
            Question
        }

        public MessageBox() => AvaloniaXamlLoader.Load(this);

        public static Task<string> Show(Window parent, string text, string title, string textException, MessageBoxIcon icon, IList<MessageBoxButton> buttons)
        {
            MessageBox msgbox = new MessageBox
            {
                Title = title,
                Icon = parent?.Icon
            };
            msgbox.FindControl<TextBlock>("Text").Text = text;
            StackPanel buttonPanel = msgbox.FindControl<StackPanel>("Buttons");
            Path iconControl = msgbox.FindControl<Path>("Icon");
            TextBox errorText = msgbox.FindControl<TextBox>("ErrorText");

            string res = "OK";

            void AddButton(MessageBoxButton r)
            {
                Button btn = new Button { Content = r.Text };
                btn.Click += (_, __) =>
                {
                    res = r.Result;
                    msgbox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (r.Def)
                    res = r.Result;
            }

            void ChangeIcon(string icn) => iconControl.Data = (Geometry)Application.Current.FindResource($"{icn}Icon");

            foreach (MessageBoxButton button in buttons)
            {
                if (button.IsKeyDown)
                    msgbox.KeyDown += (s, e) =>
                    {
                        if (e.Key == Key.Enter)
                        {
                            res = button.Result;
                            msgbox.Close();
                        }
                    };

                AddButton(button);
            }

            switch (icon)
            {
                case MessageBoxIcon.Error:
                    ChangeIcon("Error");
                    break;
                case MessageBoxIcon.Information:
                    ChangeIcon("Information");
                    break;
                case MessageBoxIcon.Question:
                    ChangeIcon("Question");
                    break;
                case MessageBoxIcon.Warning:
                    ChangeIcon("Warning");
                    break;
            }

            if (!string.IsNullOrEmpty(textException)) errorText.Text = textException;
            else errorText.IsVisible = false;

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            msgbox.Closed += delegate { _ = tcs.TrySetResult(res); };
            if (parent != null) _ = msgbox.ShowDialog(parent);
            else msgbox.Show();
            return tcs.Task;
        }
    }
}