using System;
using System.Reflection;
using PleasantUI;

namespace Regul.ModuleSystem.Structures;

/// <summary>
/// The class that contains the module instance itself and its assembly
/// </summary>
public class Module : ViewModelBase
{
    private Assembly _assembly = null!;
    private IModule _instance = null!;
    private string? _linkToUpdate;
    private bool _hasUpdate;
    private bool _readyUpgrade;
    private Version? _newVersion;
    private Version? _regulVersionRequiered;

    public IModule Instance
    {
        get => _instance;
        private init => RaiseAndSetIfChanged(ref _instance, value);
    }

    public Assembly Assembly
    {
        get => _assembly;
        private init => RaiseAndSetIfChanged(ref _assembly, value);
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

    public Module(IModule instance, Assembly moduleAssembly)
    {
        Instance = instance;
        Assembly = moduleAssembly;
    }
}