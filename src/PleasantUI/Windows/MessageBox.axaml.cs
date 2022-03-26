using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PleasantUI.Controls.Custom;
using PleasantUI.Structures;

namespace PleasantUI.Windows
{
    public class MessageBox : PleasantDialogWindow
    {
        public enum MessageBoxIcon
        {
            Information,
            Error,
            Warning,
            Question,
            None
        }

        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<string> Show(PleasantWindow parent, string title, string text,
            IList<MessageBoxButton> buttons, MessageBoxIcon icon = MessageBoxIcon.None, string textException = null)
        {
            MessageBox messageBox = new MessageBox
            {
                Title = title,
                Icon = parent?.Icon.ToBitmap()
            };

            messageBox.FindControl<TextBlock>("Text").Text = text;
            StackPanel buttonPanel = messageBox.FindControl<StackPanel>("Buttons");
            Path iconControl = messageBox.FindControl<Path>("Icon");
            TextBox errorText = messageBox.FindControl<TextBox>("ErrorText");
            ToggleButton toggleButton = messageBox.FindControl<ToggleButton>("toggleButton");

            string res = "OK";

            void AddButton(MessageBoxButton r)
            {
                Button btn = new Button { Content = r.Text, Padding = Thickness.Parse("15 0") };
                btn.Click += (_, __) =>
                {
                    res = r.Result;
                    messageBox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (r.Default)
                {
                    res = r.Result;
                    btn.Classes.Add("mbdefault");
                }
            }

            void ChangeIcon(string icn)
            {
                iconControl.Data = (Geometry)Application.Current.FindResource($"{icn}Icon");
            }

            foreach (MessageBoxButton button in buttons)
            {
                if (button.IsKeyDown)
                    messageBox.KeyDown += (s, e) =>
                    {
                        if (e.Key == Key.Enter)
                        {
                            res = button.Result;
                            messageBox.Close();
                        }
                    };

                AddButton(button);
            }

            switch (icon)
            {
                case MessageBoxIcon.Error:
                    ChangeIcon("Error");
                    iconControl.Fill = (IBrush)Application.Current.FindResource("MBErrorBrush");
                    break;
                case MessageBoxIcon.Information:
                    ChangeIcon("Information");
                    iconControl.Fill = (IBrush)Application.Current.FindResource("MBQuestionBrush");
                    break;
                case MessageBoxIcon.Question:
                    ChangeIcon("Question");
                    iconControl.Fill = (IBrush)Application.Current.FindResource("MBQuestionBrush");
                    break;
                case MessageBoxIcon.Warning:
                    ChangeIcon("Warning");
                    iconControl.Fill = (IBrush)Application.Current.FindResource("MBWarningBrush");
                    break;
            }

            if (!string.IsNullOrEmpty(textException))
                errorText.Text = textException;
            else toggleButton.IsVisible = false;

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            messageBox.Closed += (s, e) => { tcs.TrySetResult(res); };
            if (parent == null) return tcs.Task;
            messageBox.Show(parent);

            return tcs.Task;
        }
    }
}