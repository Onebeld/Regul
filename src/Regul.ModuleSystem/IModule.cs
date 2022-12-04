using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media;
using Regul.ModuleSystem.Structures;

namespace Regul.ModuleSystem;

/// <summary>
/// The module is loaded with it.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Image of the module.
    /// </summary>
    IImage? Icon { get; }

    /// <summary>
    /// Module name. Can also be a key for localization.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The name of the author, who created this module.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Module description. Can also be a key for localization.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The list of tools that your module supports. Their number can be unlimited.
    /// </summary>
    AvaloniaList<Instrument>? Instruments { get; }

    /// <summary>
    /// Module version.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Module localization
    /// </summary>
    Localization? Localization { get; }

    /// <summary>
    /// The entry point of any module.
    /// </summary>
    void Execute();

    /// <summary>
    /// If you want users to be able to change module settings, specify type here.
    /// </summary>
    Type? SettingsViewType { get; }

    /// <summary>
    /// Allows the module to implement getting a new version, thereby updating the module.
    /// </summary>
    /// <returns>The new version and a link to download the new version. If you don't want to update the module, you can return null.</returns>
    Task<Version?> GetNewVersion(out string? link, out Version? requiredRegulVersion);
}