#region

using Avalonia.Controls;
using Avalonia.Styling;

#endregion

namespace PleasantUI.Controls.Base
{
    public interface IFootered : IControl
    {
        object Footer { get; set; }
        ITemplate FooterTemplate { get; set; }
    }
}