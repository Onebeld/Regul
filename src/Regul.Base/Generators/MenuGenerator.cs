using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Regul.Base.Generators;

public static class MenuGenerator
{
    public static AvaloniaList<IAvaloniaObject> Generate(AvaloniaList<IRegulObject?> list)
    {
        // TODO: Get rid of recursion in the function GenerateMenuItems

        AvaloniaList<IAvaloniaObject> menuItems = new();

        foreach (IRegulObject? regulObject in list)
            switch (regulObject)
            {
                case RegulMenuItem regulMenuItem:
                {
                    MenuItem menuItem = new()
                    {
                        Command = regulMenuItem.Command,
                        InputGesture = regulMenuItem.Gesture!,
                        HotKey = regulMenuItem.Gesture
                    };
                    if (!string.IsNullOrEmpty(regulMenuItem.KeyIcon))
                        menuItem.Icon = new Path { Data = App.GetResource<Geometry>(regulMenuItem.KeyIcon) };
                    foreach (Binding bind in regulMenuItem.Bindings)
                        menuItem.Bind(bind.AvaloniaProperty, bind.Binder, bind.Anchor);

                    menuItem.Items = Generate(regulMenuItem.Items);

                    menuItems.Add(menuItem);
                    break;
                }
                case RegulSeparator:
                    menuItems.Add(new Separator());
                    break;
            }

        return menuItems;
    }
}