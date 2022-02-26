#region

using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

#endregion

namespace Regul.Base.Generators
{
    public class RegulMenuItem : IRegulObject
    {
        public RegulMenuItem(string id, ICommand command, KeyGesture gesture = null)
        {
            Id = id;
            Command = command;
            Gesture = gesture;
        }

        public string KeyIcon { get; set; }

        public List<Binding> Bindings { get; } = new List<Binding>();
        public AvaloniaList<IRegulObject> Items { get; } = new AvaloniaList<IRegulObject>();

        public ICommand Command { get; set; }
        public KeyGesture Gesture { get; set; }

        public IRegulObject this[string id]
        {
            get
            {
                //foreach (IRegulObject regulObject in Items)
                for (int index = 0; index < Items.Count; index++)
                {
                    IRegulObject regulObject = Items[index];

                    if (regulObject.Id == id)
                        return regulObject;
                }

                return null;
            }
        }

        public string Id { get; }

        public static AvaloniaList<IAvaloniaObject> GenerateMenuItems(AvaloniaList<IRegulObject> list)
        {
            // TODO: Get rid of recursion in the function GenerateMenuItems

            AvaloniaList<IAvaloniaObject> menuItems = new AvaloniaList<IAvaloniaObject>();

            foreach (IRegulObject regulObject in list)
                if (regulObject is RegulMenuItem regulMenuItem)
                {
                    MenuItem menuItem = new MenuItem
                    {
                        Command = regulMenuItem.Command,
                        InputGesture = regulMenuItem.Gesture,
                        HotKey = regulMenuItem.Gesture
                    };
                    if (!string.IsNullOrEmpty(regulMenuItem.KeyIcon))
                        menuItem.Icon = new Path { Data = App.GetResource<Geometry>(regulMenuItem.KeyIcon) };
                    foreach (Binding bind in regulMenuItem.Bindings)
                        menuItem.Bind(bind.AvaloniaProperty, bind.Binder, bind.Anchor);

                    menuItem.Items = GenerateMenuItems(regulMenuItem.Items);

                    menuItems.Add(menuItem);
                }
                else if (regulObject is RegulSeparator regulSeparator)
                {
                    menuItems.Add(new Separator());
                }

            return menuItems;
        }
    }
}