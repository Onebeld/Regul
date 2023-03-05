using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using PleasantUI.Controls;
using Regul.Enums;
using Regul.Managers;
using Regul.Structures;
using Regul.Views.Pages;

namespace Regul.Controls;

public class EditorsTabView : PleasantTabView, IStyleable
{
    Type IStyleable.StyleKey => typeof(PleasantTabView);

    protected override void PrepareContainerForItemOverride(Control element, object? item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);

        if (element is PleasantTabItem tabItem)
            tabItem.CloseButtonClick += TabItemOnCloseButtonClick;
    }

    private async void TabItemOnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not PleasantTabItem { DataContext: Workbench workbench } tabItem || WindowsManager.MainWindow is null) return;

        if (!workbench.IsDirty)
            tabItem.CloseCore();
        else
        {
             SaveResult saveResult = await WindowsManager.MainWindow.SaveBeforeClosingWorkbench(workbench);
             if (saveResult == SaveResult.Cancel) return;

             tabItem.CloseCore();
        }

        if (ItemCount == 0)
            WindowsManager.MainWindow.ChangePage(typeof(HomePage));
    }
}