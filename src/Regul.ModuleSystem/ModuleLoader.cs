using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Regul.ModuleSystem.Loader;

namespace Regul.ModuleSystem;

public class ModuleLoader
{
    private readonly AssemblyLoadContextBuilder _contextBuilder;
    private readonly ManagedLoadContext _context;
    private volatile bool _disposed;

    /// <summary>
    ///     Initialize an instance of <see cref="ModuleLoader" />
    /// </summary>
    /// <param name="config">The configuration for the module.</param>
    public ModuleLoader(ModuleConfig config)
    {
        ModuleConfig = config ?? throw new ArgumentNullException(nameof(config));
        _contextBuilder = CreateLoadContextBuilder(config);
        _context = _contextBuilder.Build();
    }

    public ModuleConfig ModuleConfig { get; }

    public string AssemblyPath { get; private set; }

    /// <summary>
    ///     Create a module loader for an assembly file.
    /// </summary>
    /// <param name="assemblyFile">The file path to the main assembly for the module.</param>
    /// <param name="sharedTypes">
    ///     <para>
    ///         A list of types which should be shared between the host and the module.
    ///     </para>
    /// </param>
    /// <returns>A loader.</returns>
    public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
    {
        return CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });
    }

    /// <summary>
    ///     Create a module loader for an assembly file.
    /// </summary>
    /// <param name="assemblyFile">The file path to the main assembly for the module.</param>
    /// <param name="sharedTypes">
    ///     <para>
    ///         A list of types which should be shared between the host and the module.
    ///     </para>
    /// </param>
    /// <param name="configure">A function which can be used to configure advanced options for the module loader.</param>
    /// <returns>A loader.</returns>
    public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes,
        Action<ModuleConfig> configure)
    {
        return CreateFromAssemblyFile(assemblyFile,
            config =>
            {
                if (sharedTypes != null)
                {
                    HashSet<Assembly> uniqueAssemblies = new();
                    foreach (Type type in sharedTypes) uniqueAssemblies.Add(type.Assembly);

                    foreach (Assembly assembly in uniqueAssemblies) config.SharedAssemblies.Add(assembly.GetName());
                }

                configure(config);
            });
    }

    /// <summary>
    ///     Create a module loader for an assembly file.
    /// </summary>
    /// <param name="assemblyFile">The file path to the main assembly for the module.</param>
    /// <returns>A loader.</returns>
    public static ModuleLoader CreateFromAssemblyFile(string assemblyFile)
    {
        return CreateFromAssemblyFile(assemblyFile, _ => { });
    }

    /// <summary>
    ///     Create a module loader for an assembly file.
    /// </summary>
    /// <param name="assemblyFile">The file path to the main assembly for the module.</param>
    /// <param name="configure">A function which can be used to configure advanced options for the module loader.</param>
    /// <returns>A loader.</returns>
    public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, Action<ModuleConfig> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        ModuleConfig config = new(assemblyFile);
        configure(config);
        return new ModuleLoader(config);
    }

    /// <summary>
    ///     Load the main assembly for the module.
    /// </summary>
    public Assembly LoadDefaultAssembly()
    {
        EnsureNotDisposed();
        return _context.LoadAssemblyFromFilePath(ModuleConfig.MainAssemblyPath);
    }

    /// <summary>
    ///     Load an assembly by name.
    /// </summary>
    /// <param name="assemblyName">The assembly name.</param>
    /// <returns>The assembly.</returns>
    public Assembly LoadAssembly(AssemblyName assemblyName)
    {
        EnsureNotDisposed();
        //return _context.LoadFromAssemblyName(assemblyName);
        return Assembly.Load(assemblyName);
    }

    /// <summary>
    ///     Load an assembly from path.
    /// </summary>
    /// <param name="assemblyPath">The assembly path.</param>
    /// <returns>The assembly.</returns>
    public Assembly LoadAssemblyFromPath(string assemblyPath)
    {
        AssemblyPath = assemblyPath;
        return _context.LoadAssemblyFromFilePath(assemblyPath);
    }

    /// <summary>
    ///     Load an assembly by name.
    /// </summary>
    /// <param name="assemblyName">The assembly name.</param>
    /// <returns>The assembly.</returns>
    public Assembly LoadAssembly(string assemblyName)
    {
        EnsureNotDisposed();
        return LoadAssembly(new AssemblyName(assemblyName));
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ModuleLoader));
    }

    private static AssemblyLoadContextBuilder CreateLoadContextBuilder(ModuleConfig config)
    {
        AssemblyLoadContextBuilder builder = new();

        builder.SetMainAssemblyPath(config.MainAssemblyPath);

        foreach (AssemblyName ext in config.PrivateAssemblies) builder.PreferLoadContextAssembly(ext);

        if (config.PreferSharedTypes) builder.PreferDefaultLoadContext(true);

        builder.IsLazyLoaded(config.IsLazyLoaded);
        foreach (AssemblyName assemblyName in config.SharedAssemblies)
            builder.PreferDefaultLoadContextAssembly(assemblyName);

#if !FEATURE_NATIVE_RESOLVER

        // In .NET Core 3.0, this code is unnecessary because the API, AssemblyDependencyResolver, handles parsing these files.
        string baseDir = Path.GetDirectoryName(config.MainAssemblyPath);
        string assemblyFileName = Path.GetFileNameWithoutExtension(config.MainAssemblyPath);

        string depsJsonFile = Path.Combine(baseDir, assemblyFileName + ".deps.json");
        if (File.Exists(depsJsonFile)) builder.AddDependencyContext(depsJsonFile);

        string moduleRuntimeConfigFile = Path.Combine(baseDir, assemblyFileName + ".runtimeconfig.json");

        builder.TryAddAdditionalProbingPathFromRuntimeConfig(moduleRuntimeConfigFile, true, out _);

        // Always include runtimeconfig.json from the host app.
        // in some cases, like `dotnet test`, the entry assembly does not actually match with the
        // runtime config file which is why we search for all files matching this extensions.
        foreach (string runtimeconfig in Directory.GetFiles(AppContext.BaseDirectory, "*.runtimeconfig.json"))
            builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeconfig, true, out _);
#endif

        return builder;
    }
}