using System;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Styling;
using PleasantUI.Controls;
using Regul.Generators;

namespace Regul.Controls;

public class EditorsTabView : PleasantTabView, IStyleable
{
    Type IStyleable.StyleKey => typeof(PleasantTabView);
    
    protected override IItemContainerGenerator CreateItemContainerGenerator()
    {
        return new EditorsItemContainerGenerator(
            this,
            ContentControl.ContentProperty,
            ContentControl.ContentTemplateProperty);
    }
}