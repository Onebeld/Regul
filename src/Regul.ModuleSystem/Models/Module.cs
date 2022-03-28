using System.Reflection;
using Onebeld.Extensions;
using Onebeld.Plugins;

namespace Regul.ModuleSystem.Models;

public class Module : ViewModelBase
{
    private InfoForUpdate _infoForUpdate;
    private Assembly _moduleAssembly;
    private PluginLoader _pluginLoader;
    private IModule _source;

    public Module(IModule source, PluginLoader pluginLoader, Assembly moduleAssembly, InfoForUpdate infoForUpdate)
    {
        _infoForUpdate = infoForUpdate;
        _moduleAssembly = moduleAssembly;
        _pluginLoader = pluginLoader;
        _source = source;
    }

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