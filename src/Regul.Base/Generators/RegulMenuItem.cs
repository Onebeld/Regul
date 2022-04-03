using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace Regul.Base.Generators;

public class RegulMenuItem : IRegulObject
{
    public RegulMenuItem(string id, ICommand? command, KeyGesture? gesture = null)
    {
        Id = id;
        Command = command;
        Gesture = gesture;
    }

    public string KeyIcon { get; set; } = null!;

    public List<Binding> Bindings { get; } = new();
    public AvaloniaList<IRegulObject?> Items { get; } = new();

    public ICommand? Command { get; set; }
    public KeyGesture? Gesture { get; set; }

    public IRegulObject? this[string id]
    {
        get
        {
            //foreach (IRegulObject regulObject in Items)
            for (int index = 0; index < Items.Count; index++)
            {
                IRegulObject? regulObject = Items[index];

                if (regulObject?.Id == id)
                    return regulObject;
            }

            return null;
        }
    }

    public string Id { get; }
}