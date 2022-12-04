using System;
using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using Regul.ModuleSystem.Structures;
using Module = Regul.ModuleSystem.Structures.Module;

namespace Regul.ModuleSystem;

/// <summary>
/// ModuleManager allows you to manage modules
/// </summary>
public static class ModuleManager
{
    /// <summary>
    /// List of loaded modules in the program
    /// </summary>
    public static AvaloniaList<Module> Modules { get; } = new();

    /// <summary>
    /// The list of loaded editors in the program. You must add an editor to the list yourself.
    /// </summary>
    public static AvaloniaList<Editor> Editors { get; } = new();

    // <summary>
    /// Logic when closing the program. Recommended if the module has any settings and you want to save them.
    /// </summary>
    public static event EventHandler? ReleasingModules;

    /// <summary>
    /// Allows you to get an instance of an <see cref="Editor"/> by its ID
    /// </summary>
    /// <param name="id">Editor ID</param>
    /// <returns>Editor's copy</returns>
    public static Editor? GetEditorById(ulong id) => Editors.FirstOrDefault(editor => editor.Id == id);

    /// <summary>
    /// Allows you to load a module into the program
    /// </summary>
    /// <param name="path">The path to the module</param>
    /// <returns>A module instance, it is already automatically added to the list of modules.</returns>
    public static Module? InitializeModule(string path)
    {
        ModuleLoader moduleLoader = ModuleLoader.CreateFromAssemblyFile(path);

        Assembly assembly = moduleLoader.LoadDefaultAssembly();

        Type?[] types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).ToArray();
        }

        IModule? source = null;

        foreach (Type? type in types)
        {
            if (type != null && typeof(IModule).IsAssignableFrom(type))
            {
                source = Activator.CreateInstance(type) as IModule;
                break;
            }
        }

        if (source is null) return null;

        Module module = new(source, assembly);
        Modules.Add(module);

        return module;
    }

    public static void ReleaseModules() => ReleasingModules?.Invoke(null, EventArgs.Empty);
}