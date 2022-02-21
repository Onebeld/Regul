using System.Reflection;
using Onebeld.Extensions;
using Onebeld.Plugins;

namespace Regul.ModuleSystem.Models
{
	public class Module : ViewModelBase
	{
		private PluginLoader _pluginLoader;
		private IModule _source;
		private InfoForUpdate _infoForUpdate;
		private Assembly _moduleAssembly;

		public PluginLoader PluginLoader
		{
			get => _pluginLoader;
			set => RaiseAndSetIfChanged(ref _pluginLoader, value);
		}
		public IModule Source
		{
			get => _source;
			set => RaiseAndSetIfChanged(ref _source, value);
		}
		public InfoForUpdate InfoForUpdate
		{
			get => _infoForUpdate;
			set => RaiseAndSetIfChanged(ref _infoForUpdate, value);
		}

		public Assembly ModuleAssembly
		{
			get => _moduleAssembly;
			set => RaiseAndSetIfChanged(ref _moduleAssembly, value);
		}
	}
}
