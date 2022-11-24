using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using PleasantUI.Controls;
using PleasantUI.Interfaces;
using PleasantUI.Structures;

namespace PleasantUI.Windows;

public class MessageBox : ContentDialog
{
    public MessageBox() => AvaloniaXamlLoader.Load(this);

    public static Task<string> Show(IPleasantWindowModal parent, string title, string text,
        IList<MessageBoxButton>? buttons = null, string? additionalText = null)
    {
        MessageBox messageBox = new();
        
        string titleValue, textValue;
        
        if (Application.Current!.TryFindResource(title, out object? objectTitleValue))
            titleValue = objectTitleValue as string ?? string.Empty;
        else titleValue = title;
        if (Application.Current!.TryFindResource(text, out object? objectTextValue))
            textValue = objectTextValue as string ?? string.Empty;
        else textValue = text;

        messageBox.FindControl<TextBlock>("Title")!.Text = titleValue;
        messageBox.FindControl<TextBlock>("Text")!.Text = textValue;
        TextBox textBox = messageBox.FindControl<TextBox>("AdditionalText")!;
        UniformGrid uniformGrid = messageBox.FindControl<UniformGrid>("Buttons")!;

        string result = "OK";

        void AddButton(MessageBoxButton messageBoxButton)
        {
            string textValue;
            
            if (Application.Current!.TryFindResource(messageBoxButton.Text, out object? objectTextValue))
                textValue = objectTextValue as string ?? string.Empty;
            else textValue = messageBoxButton.Text;
            
            Button button = new()
            {
                Content = textValue, 
                Margin = Thickness.Parse("5"),
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_, _) =>
            {
                result = messageBoxButton.Result;
                messageBox.Close();
            };
            uniformGrid.Children.Add(button);

            if (messageBoxButton.IsKeyDown)
            {
                messageBox.KeyDown += (_, e) =>
                {
                    if (e.Key != Key.Enter) return;

                    result = messageBoxButton.Result;
                    messageBox.Close();
                };
            }

            if (!messageBoxButton.Default) return;
            result = messageBoxButton.Result;
            button.Classes.Add("Accent");
            
            if (Application.Current != null)
                button.Theme = (ControlTheme)Application.Current.FindResource("AccentButtonTheme")!;
        }

        if (buttons is null || buttons.Count == 0)
        {
            uniformGrid.Columns = 2;
            uniformGrid.Children.Add(new Panel());
            AddButton(new MessageBoxButton
            {
                Text = (string)Application.Current!.FindResource("Ok")!,
                Result = "OK",
                Default = true,
                IsKeyDown = true
            });
        }
        else if (buttons.Count == 1)
        {
            uniformGrid.Columns = 2;
            uniformGrid.Children.Add(new Panel());
            
            AddButton(buttons[0]);
        }
        else
        {
            uniformGrid.Columns = buttons.Count;

            foreach (MessageBoxButton messageBoxButton in buttons) 
                AddButton(messageBoxButton);
        }

        if (!string.IsNullOrWhiteSpace(additionalText))
            textBox.Text = additionalText;
        else textBox.IsVisible = false;

        TaskCompletionSource<string> taskCompletionSource = new();
        messageBox.Closed += (_, _) => taskCompletionSource.TrySetResult(result);
        messageBox.Show(parent);

        return taskCompletionSource.Task;
    }
}