// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Onebeld.Plugins.Loader;

#endregion

namespace Onebeld.Plugins
{
	/// <summary>
	///     This loader attempts to load binaries for execution (both managed assemblies and native libraries)
	///     in the same way that .NET Core would if they were originally part of the .NET Core application.
	///     <para>
	///         This loader reads configuration files produced by .NET Core (.deps.json and runtimeconfig.json)
	///         as well as a custom file (*.config files). These files describe a list of .dlls and a set of dependencies.
	///         The loader searches the plugin path, as well as any additionally specified paths, for binaries
	///         which satisfy the plugin's requirements.
	///     </para>
	/// </summary>
	public class PluginLoader
    {
        private readonly AssemblyLoadContextBuilder _contextBuilder;
        private readonly ManagedLoadContext _context;
        private volatile bool _disposed;

        /// <summary>
        ///     Initialize an instance of <see cref="PluginLoader" />
        /// </summary>
        /// <param name="config">The configuration for the plugin.</param>
        public PluginLoader(PluginConfig config)
        {
            PluginConfig = config ?? throw new ArgumentNullException(nameof(config));
            _contextBuilder = CreateLoadContextBuilder(config);
            _context = _contextBuilder.Build();
        }

        public PluginConfig PluginConfig { get; }

        public string AssemblyPath { get; private set; }

        /// <summary>
        ///     Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="sharedTypes">
        ///     <para>
        ///         A list of types which should be shared between the host and the plugin.
        ///     </para>
        ///     <para>
        ///         <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md">
        ///             https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md
        ///         </seealso>
        ///     </para>
        /// </param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
        {
            return CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });
        }

        /// <summary>
        ///     Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="sharedTypes">
        ///     <para>
        ///         A list of types which should be shared between the host and the plugin.
        ///     </para>
        ///     <para>
        ///         <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md">
        ///             https://github.com/natemcmaster/DotNetCorePlugins/blob/main/docs/what-are-shared-types.md
        ///         </seealso>
        ///     </para>
        /// </param>
        /// <param name="configure">A function which can be used to configure advanced options for the plugin loader.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes,
            Action<PluginConfig> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                config =>
                {
                    if (sharedTypes != null)
                    {
                        HashSet<Assembly> uniqueAssemblies = new HashSet<Assembly>();
                        foreach (Type type in sharedTypes) uniqueAssemblies.Add(type.Assembly);

                        foreach (Assembly assembly in uniqueAssemblies) config.SharedAssemblies.Add(assembly.GetName());
                    }

                    configure(config);
                });
        }

        /// <summary>
        ///     Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile)
        {
            return CreateFromAssemblyFile(assemblyFile, _ => { });
        }

        /// <summary>
        ///     Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="configure">A function which can be used to configure advanced options for the plugin loader.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Action<PluginConfig> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            PluginConfig config = new PluginConfig(assemblyFile);
            configure(config);
            return new PluginLoader(config);
        }

        /// <summary>
        ///     Load the main assembly for the plugin.
        /// </summary>
        public Assembly LoadDefaultAssembly()
        {
            EnsureNotDisposed();
            return _context.LoadAssemblyFromFilePath(PluginConfig.MainAssemblyPath);
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
            if (_disposed) throw new ObjectDisposedException(nameof(PluginLoader));
        }

        private static AssemblyLoadContextBuilder CreateLoadContextBuilder(PluginConfig config)
        {
            AssemblyLoadContextBuilder builder = new AssemblyLoadContextBuilder();

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

            string pluginRuntimeConfigFile = Path.Combine(baseDir, assemblyFileName + ".runtimeconfig.json");

            builder.TryAddAdditionalProbingPathFromRuntimeConfig(pluginRuntimeConfigFile, true, out _);

            // Always include runtimeconfig.json from the host app.
            // in some cases, like `dotnet test`, the entry assembly does not actually match with the
            // runtime config file which is why we search for all files matching this extensions.
            foreach (string runtimeconfig in Directory.GetFiles(AppContext.BaseDirectory, "*.runtimeconfig.json"))
                builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeconfig, true, out _);
#endif

            return builder;
        }
    }
}