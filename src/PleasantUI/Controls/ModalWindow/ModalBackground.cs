using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

[PseudoClasses(":close")]
internal class ModalBackground : TemplatedControl
{
    public void Close()
    {
        PseudoClasses.Set(":close", true);
    }
}