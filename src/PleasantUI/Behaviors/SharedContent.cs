#region

using Avalonia.Controls;
using Avalonia.Metadata;

#endregion

namespace PleasantUI.Behaviors
{
    public class SharedContent : Control
    {
        [Content] public object Content { get; set; }
    }
}