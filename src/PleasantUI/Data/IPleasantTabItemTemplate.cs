using Avalonia.Controls.Shapes;

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