using Avalonia;
using Avalonia.Data;

namespace Regul.Base.Generators
{
    public class Binding
    {
        public AvaloniaProperty AvaloniaProperty { get; }
        public IBinding Binder { get; }
        public object Anchor { get; }
        
        public Binding(AvaloniaProperty property, IBinding binding, object anchor = null)
        {
            AvaloniaProperty = property;
            Binder = binding;
            Anchor = anchor;
        }
    }
}