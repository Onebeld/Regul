using Avalonia.Controls.Shapes;

namespace PleasantUI.Data;

public interface IPleasantTabItemTemplate
{
    bool IsClosable { get; }
    Path Icon { get; }
}