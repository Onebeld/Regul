#region

using System;
using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using Onebeld.Extensions;
using Onebeld.Plugins;
using Regul.ModuleSystem.Models;
using Module = Regul.ModuleSystem.Models.Module;

#endregion

namespace Regul.ModuleSystem
{
    public class ModuleManager : ViewModelBase
    {
        public static AvaloniaList<Module> Modules { get; } = new AvaloniaList<Module>();

        public static AvaloniaList<Editor> Editors { get; } = new AvaloniaList<Editor>();

        public static Editor GetEditorById(string id)
        {
            //return Editors.FirstOrDefault(x => x.Id == id);
            for (int i = 0; i < Editors.Count; i++)
            {
                Editor item = Editors[i];

                if (item.Id == id)
                    return item;
            }

            return null;
        }

        public static Module InitializeModule(string path)
        {
            bool suitableType = false;

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

            IModule source = null;

            foreach (Type type in types)
                if (typeof(IModule).IsAssignableFrom(type))
                {
                    source = Activator.CreateInstance(type) as IModule;
                    suitableType = true;
                }

            if (!suitableType) return null;

            Module module = new Module
            {
                PluginLoader = loader,
                Source = source,
                InfoForUpdate = new InfoForUpdate(),
                ModuleAssembly = assembly
            };

            Modules.Add(module);
            return module;
        }
    }
}