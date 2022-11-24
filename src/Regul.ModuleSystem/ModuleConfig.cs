using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Regul.ModuleSystem;

/// <summary>
///     Represents the configuration for a .NET Core module.
/// </summary>
public class ModuleConfig
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ModuleConfig" />
    /// </summary>
    /// <param name="mainAssemblyPath">The full file path to the main assembly for the module.</param>
    public ModuleConfig(string mainAssemblyPath)
    {
        if (string.IsNullOrEmpty(mainAssemblyPath))
            throw new ArgumentException("Value must be null or not empty", nameof(mainAssemblyPath));

        if (!Path.IsPathRooted(mainAssemblyPath))
            throw new ArgumentException("Value must be an absolute file path", nameof(mainAssemblyPath));

        MainAssemblyPath = mainAssemblyPath;
    }

    /// <summary>
    ///     The file path to the main assembly.
    /// </summary>
    public string MainAssemblyPath { get; }

    /// <summary>
    ///     A list of assemblies which should be treated as private.
    /// </summary>
    public ICollection<AssemblyName> PrivateAssemblies { get; protected set; } = new List<AssemblyName>();

    /// <summary>
    ///     A list of assemblies which should be unified between the host and the module.
    /// </summary>
    public ICollection<AssemblyName> SharedAssemblies { get; protected set; } = new List<AssemblyName>();

    /// <summary>
    ///     Attempt to unify all types from a module with the host.
    ///     <para>
    ///         This does not guarantee types will unify.
    ///     </para>
    /// </summary>
    public bool PreferSharedTypes { get; set; }

    /// <summary>
    ///     If enabled, will lazy load dependencies of all shared assemblies.
    ///     Reduces module load time at the expense of non-determinism in how transitive dependencies are loaded
    ///     between the module and the host.
    /// </summary>
    public bool IsLazyLoaded { get; set; } = false;
}