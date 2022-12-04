using System;
using McMaster.NETCore.Plugins;
using PleasantUI;

namespace Regul.ModuleSystem.Structures;

/// <summary>
/// The class that contains the module instance itself and its assembly
/// </summary>
public class Module : ViewModelBase
{
    private IModule _instance = null!;
    private string? _linkToUpdate;
    private bool _hasUpdate;
    private bool _readyUpgrade;
    private Version? _newVersion;
    private Version? _regulVersionRequiered;
    private PluginLoader _pluginLoader;

    public IModule Instance
    {
        get => _instance;
        private init => RaiseAndSetIfChanged(ref _instance, value);
    }

    public PluginLoader PluginLoader
    {
        get => _pluginLoader;
        set => RaiseAndSetIfChanged(ref _pluginLoader, value);
    }

    public string? LinkToUpdate
    {
        get => _linkToUpdate;
        set => RaiseAndSetIfChanged(ref _linkToUpdate, value);
    }
    public bool HasUpdate
    {
        get => _hasUpdate;
        set => RaiseAndSetIfChanged(ref _hasUpdate, value);
    }
    public bool ReadyUpgrade
    {
        get => _readyUpgrade;
        set => RaiseAndSetIfChanged(ref _readyUpgrade, value);
    }
    public Version? NewVersion
    {
        get => _newVersion;
        set => RaiseAndSetIfChanged(ref _newVersion, value);
    }
    public Version? RegulVersionRequiered
    {
        get => _regulVersionRequiered;
        set => RaiseAndSetIfChanged(ref _regulVersionRequiered, value);
    }

    public Module(IModule instance, PluginLoader pluginLoader)
    {
        Instance = instance;
        PluginLoader = pluginLoader;
    }
}