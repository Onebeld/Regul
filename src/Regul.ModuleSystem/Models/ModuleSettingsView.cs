using System;
using Avalonia.Media;

namespace Regul.ModuleSystem.Models;

public class ModuleSettingsView
{
    public ModuleSettingsView(string moduleName, IImage? moduleImage, Func<IModuleSettings> settings)
    {
        ModuleName = moduleName;
        ModuleImage = moduleImage;
        Settings = settings;
    }

    public string ModuleName { get; set; }
    public IImage? ModuleImage { get; set; }
    
    public Func<IModuleSettings> Settings { get; set; }
}