using Avalonia.Collections;
using Onebeld.Extensions;
using Onebeld.Plugins;
using System;
using System.Linq;
using System.Reflection;

namespace Regul.ModuleSystem
{
	public class ModuleManager : ViewModelBase
	{
		private AvaloniaList<IModule> _modules = new AvaloniaList<IModule>();

		public static ModuleManager System = new ModuleManager();

		public AvaloniaList<IModule> Modules
		{
			get => _modules;
			set => RaiseAndSetIfChanged(ref _modules, value);
		}

		public static AvaloniaList<Editor> Editors { get; } = new AvaloniaList<Editor>();

		public static IModule InitializeModule(string path)
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

			IModule module = null;

			foreach (Type type in types)
			{
				if (typeof(IModule).IsAssignableFrom(type))
				{
					module = Activator.CreateInstance(type) as IModule;
					System.Modules.Add(module);
					suitableType = true;
				}
			}

			if (!suitableType)
			{
				loader.Dispose();
				return null;
			}

			return module;
		}
	}
}
