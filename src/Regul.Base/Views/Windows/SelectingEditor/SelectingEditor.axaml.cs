﻿using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls.Custom;
using Regul.ModuleSystem;

namespace Regul.Base.Views.Windows;

public class SelectingEditor : PleasantDialogWindow
{
    public SelectingEditor() => AvaloniaXamlLoader.Load(this);

    public static async Task<(Editor?, bool)> GetEditor(string? fileName, bool showCheckBox = true)
    {
        SelectingEditor? selectingEditor = WindowsManager.CreateModalWindow<SelectingEditor>();

        if (selectingEditor == null)
            return (null, false);

        selectingEditor.FindControl<TextBlock>("PART_FileName").Text = fileName;

        CheckBox checkBox = selectingEditor.FindControl<CheckBox>("PART_AlwaysOpen");
        checkBox.IsVisible = showCheckBox;

        selectingEditor.FindControl<ListBox>("ListBox").Focus();
            
        Editor? editor = await selectingEditor.Show<Editor?>(WindowsManager.MainWindow);

        return (editor, editor != null && (checkBox.IsChecked ?? false));
    }
}