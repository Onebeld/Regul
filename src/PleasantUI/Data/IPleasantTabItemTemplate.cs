#region

using Avalonia.Controls.Shapes;

#endregion

namespace PleasantUI.Data
{
    public interface IPleasantTabItemTemplate
    {
        object Content { get; }
        object Header { get; }
        bool IsClosable { get; }
        Path Icon { get; }
    }
}