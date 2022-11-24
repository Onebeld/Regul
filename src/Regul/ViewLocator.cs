#if DEBUG

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PleasantUI;

namespace Regul
{
    public class ViewLocator : IDataTemplate
    {
        public IControl Build(object? data)
        {
            string? name = data?.GetType().FullName!.Replace("ViewModel", "View");
            Type? type = Type.GetType(name!);

            if (type != null)
            {
                try
                {
                    return (Control)Activator.CreateInstance(type)!;
                }
                catch
                {
                    return new Control();
                }
                
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}

#endif