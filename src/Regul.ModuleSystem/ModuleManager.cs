using System;
using System.Linq;
using Avalonia.Collections;
using McMaster.NETCore.Plugins;
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

    /// <summary>
    /// Allows you to get an instance of an <see cref="Editor"/> by its ID
    /// </summary>
    /// <param name="id">Editor ID</param>
    /// <returns>Editor's copy</returns>
    public static Editor? GetEditorById(string id) => Editors.FirstOrDefault(editor => editor.Id == id);

    public static void RemoveEditorById(string id)
    {
        Editor? editor = GetEditorById(id);
        if (editor is not null)
            Editors.Remove(editor);
    }

    /// <summary>
    /// Allows you to load a module into the program
    /// </summary>
    /// <param name="path">The path to the module</param>
    /// <returns>A module instance, it is already automatically added to the list of modules.</returns>
    public static Module? InitializeModule(string path)
    {
        PluginLoader moduleLoader = PluginLoader.CreateFromAssemblyFile(path, x =>
        {
            x.IsUnloadable = true;
            x.LoadInMemory = true;
        });
        IModule? source = null;

        try
        {
            foreach (Type type in moduleLoader
                         .LoadDefaultAssembly()
                         .GetTypes()
                         .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract))
            {
                source = (IModule?)Activator.CreateInstance(type);
            }
        }
        catch (Exception e)
        {
            moduleLoader.Dispose();
            throw;
        }

        if (source is null)
        {
            moduleLoader.Dispose();
            return null;
        }

        Module module = new(source, moduleLoader);
        Modules.Add(module);

        return module;
    }
}