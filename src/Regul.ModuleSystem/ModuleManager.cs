using System;
using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using Onebeld.Extensions;
using Onebeld.Plugins;
using Regul.ModuleSystem.Models;
using Module = Regul.ModuleSystem.Models.Module;

namespace Regul.ModuleSystem;

public class ModuleManager : ViewModelBase
{
    public static AvaloniaList<Module> Modules { get; } = new();

    public static AvaloniaList<Editor?> Editors { get; } = new();

    public static Editor? GetEditorById(string? id)
    {
        //return Editors.FirstOrDefault(x => x.Id == id);
        for (int i = 0; i < Editors.Count; i++)
        {
            Editor? item = Editors[i];

            if (item?.Id == id)
                return item;
        }

        return null;
    }

    public static Module? InitializeModule(string path)
    {
        PluginLoader loader = PluginLoader.CreateFromAssemblyFile(path);

        Assembly assembly = loader.LoadDefaultAssembly();

        Type[] types;

        // This was done to run both the .NET and the .NET Framework simultaneously
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).ToArray();
        }

        IModule? source = null;

        foreach (Type type in types)
            if (typeof(IModule).IsAssignableFrom(type))
            {
                source = Activator.CreateInstance(type) as IModule;
                break;
            }

        if (source is null) return null;

        Module module = new(source, loader, assembly, new InfoForUpdate());

        Modules.Add(module);
        return module;
    }
}