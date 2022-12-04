using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.Controls;
using PleasantUI.Generators;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Enums;
using Regul.Managers;
using Regul.Structures;
using Regul.Views.Pages;

namespace Regul.Generators;

public class EditorsItemContainerGenerator : PleasantTabItemContainerGenerator
{
    public EditorsItemContainerGenerator(PleasantTabView owner, AvaloniaProperty contentProperty, AvaloniaProperty contentTemplateProperty) : base(owner, contentProperty, contentTemplateProperty)
    {
    }

    protected override IControl CreateContainer(object item)
    {
        if (base.CreateContainer(item) is PleasantTabItem tabItem)
        {
            tabItem.CloseButtonClick += TabItemOnCloseButtonClick;

            return tabItem;
        }

        throw new NullReferenceException();
    }

    private async void TabItemOnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not PleasantTabItem { DataContext: Workbench workbench } tabItem || WindowsManager.MainWindow is null) return;

        if (!workbench.IsDirty)
        {
            tabItem.CloseCore();

            if (((PleasantTabView)Owner).ItemCount == 0)
                WindowsManager.MainWindow.ChangePage(typeof(HomePage));

            return;
        }

        SaveResult saveResult = await WindowsManager.MainWindow.SaveBeforeClosingWorkbench(workbench);
        
        if (saveResult == SaveResult.Cancel) return;

        tabItem.CloseCore();

        if (((PleasantTabView)Owner).ItemCount == 0)
            WindowsManager.MainWindow.ChangePage(typeof(HomePage));
    }
}