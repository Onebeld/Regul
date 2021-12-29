using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Regul.Base.Other
{
    public static class Extensions
    {
        /// <summary>
        /// Gets an <typeparamref name="T"/> with the given binding
        /// </summary>
        public static T Bind<T>(this IAvaloniaObject control, AvaloniaProperty property, IBinding binding, object anchor = null)
		{
            control.Bind(property, binding, anchor);
            return (T)control;
		}
        /// <summary>
        /// Gets an <typeparamref name="T"/> with the given converter
        /// </summary>
        public static Binding Converter(this Binding binding, IValueConverter converter)
        {
            binding.Converter = converter;
            return binding;
        }
    }
}