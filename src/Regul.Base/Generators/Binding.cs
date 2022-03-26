using Avalonia;
using Avalonia.Data;

namespace Regul.Base.Generators
{
    public class Binding
    {
        public Binding(AvaloniaProperty property, IBinding binding, object anchor = null)
        {
            AvaloniaProperty = property;
            Binder = binding;
            Anchor = anchor;
        }

        public AvaloniaProperty AvaloniaProperty { get; }
        public IBinding Binder { get; }
        public object Anchor { get; }
    }
}