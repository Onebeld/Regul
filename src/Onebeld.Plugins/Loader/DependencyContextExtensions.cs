// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyModel;
using Onebeld.Plugins.LibraryModel;
using NativeLibrary = Onebeld.Plugins.LibraryModel.NativeLibrary;

namespace Onebeld.Plugins.Loader
{
    /// <summary>
    /// Extensions for configuring a load context using .deps.json files.
    /// </summary>
    public static class DependencyContextExtensions
    {
        /// <summary>
        /// Add dependency information to a load context from a .deps.json file.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="depsFilePath">The full path to the .deps.json file.</param>
        /// <param name="error">An error, if one occurs while reading .deps.json</param>
        /// <returns>The builder.</returns>
        public static AssemblyLoadContextBuilder TryAddDependencyContext(this AssemblyLoadContextBuilder builder, string depsFilePath, out Exception error)
        {
            error = null;
            try
            {
                builder.AddDependencyContext(depsFilePath);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            return builder;
        }

        /// <summary>
        /// Add dependency information to a load context from a .deps.json file.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="depsFilePath">The full path to the .deps.json file.</param>
        /// <returns>The builder.</returns>
        public static AssemblyLoadContextBuilder AddDependencyContext(this AssemblyLoadContextBuilder builder, string depsFilePath)
        {

            DependencyContextJsonReader reader = new DependencyContextJsonReader();
            using (FileStream file = File.OpenRead(depsFilePath))
            {
                DependencyContext deps = reader.Read(file);
                builder.AddDependencyContext(deps);
            }

            return builder;
        }

        private static string GetFallbackRid()
        {
            // see https://github.com/dotnet/core-setup/blob/b64f7fffbd14a3517186b9a9d5cc001ab6e5bde6/src/corehost/common/pal.h#L53-L73

            string ridBase;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ridBase = "win10";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ridBase = "linux";

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ridBase = "osx.10.12";
            }
            else
            {
                return "any";
            }

            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.Arm:
                    return ridBase + "-arm";
                case Architecture.Arm64:
                    return ridBase + "-arm64";
                case Architecture.X64:
                    return ridBase + "-x64";
                case Architecture.X86:
                    return ridBase + "-x86";
                default:
                    return ridBase;
            }
        }

        /// <summary>
        /// Add a pre-parsed <see cref="DependencyContext" /> to the load context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="dependencyContext">The dependency context.</param>
        /// <returns>The builder.</returns>
        public static AssemblyLoadContextBuilder AddDependencyContext(this AssemblyLoadContextBuilder builder, DependencyContext dependencyContext)
        {
            IReadOnlyList<RuntimeFallbacks> ridGraph = dependencyContext.RuntimeGraph.Any() || DependencyContext.Default == null
               ? dependencyContext.RuntimeGraph
               : DependencyContext.Default.RuntimeGraph;

            string rid = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            string fallbackRid = GetFallbackRid();
            RuntimeFallbacks fallbackGraph = ridGraph.FirstOrDefault(g => g.Runtime == rid)
                                             ?? ridGraph.FirstOrDefault(g => g.Runtime == fallbackRid)
                                             ?? new RuntimeFallbacks("any");

            foreach (ManagedLibrary managed in dependencyContext.ResolveRuntimeAssemblies(fallbackGraph))
            {
                builder.AddManagedLibrary(managed);
            }

            foreach (RuntimeLibrary library in dependencyContext.ResolveResourceAssemblies())
            {
                foreach (ResourceAssembly resource in library.ResourceAssemblies)
                {
                    /*
                     * For resource assemblies, look in $packageRoot/$packageId/$version/$resourceGrandparent
                     *
                     * For example, a deps file may contain
                     *
                     * "Example/1.0.0": {
                     *    "runtime": {
                     *         "lib/netcoreapp2.0/Example.dll": { }
                     *     },
                     *     "resources": {
                     *         "lib/netcoreapp2.0/es/Example.resources.dll": {
                     *           "locale": "es"
                     *         }
                     *     }
                     * }
                     *
                     * In this case, probing should happen in $packageRoot/example/1.0.0/lib/netcoreapp2.0
                     */

                    string resourceDir = Path.GetDirectoryName(Path.GetDirectoryName(resource.Path));

                    if (resourceDir != null)
                    {
                        string path = Path.Combine(library.Name.ToLowerInvariant(),
                            library.Version,
                            resourceDir);

                        builder.AddResourceProbingSubpath(path);
                    }
                }
            }

            foreach (NativeLibrary native in dependencyContext.ResolveNativeAssets(fallbackGraph))
            {
                builder.AddNativeLibrary(native);
            }

            return builder;
        }

        private static IEnumerable<ManagedLibrary> ResolveRuntimeAssemblies(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
        {
            IEnumerable<string> rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.RuntimeAssemblyGroups)
                   select ManagedLibrary.CreateFromPackage(library.Name, library.Version, assetPath);
        }

        private static IEnumerable<RuntimeLibrary> ResolveResourceAssemblies(this DependencyContext depContext)
        {
            return from library in depContext.RuntimeLibraries
                   where library.ResourceAssemblies != null && library.ResourceAssemblies.Count > 0
                   select library;
        }

        private static IEnumerable<NativeLibrary> ResolveNativeAssets(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
        {
            IEnumerable<string> rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.NativeLibraryGroups)
                       // some packages include symbols alongside native assets, such as System.Native.a or pwshplugin.pdb
                   where PlatformInformation.NativeLibraryExtensions.Contains(Path.GetExtension(assetPath), StringComparer.OrdinalIgnoreCase)
                   select NativeLibrary.CreateFromPackage(library.Name, library.Version, assetPath);
        }

        private static IEnumerable<string> GetRids(RuntimeFallbacks runtimeGraph)
        {
            return new[] { runtimeGraph.Runtime }.Concat(runtimeGraph?.Fallbacks ?? Enumerable.Empty<string>());
        }

        private static IEnumerable<string> SelectAssets(IEnumerable<string> rids, IEnumerable<RuntimeAssetGroup> groups)
        {
            foreach (string rid in rids)
            {
                RuntimeAssetGroup group = groups.FirstOrDefault(g => g.Runtime == rid);
                if (group != null)
                {
                    return group.AssetPaths;
                }
            }

            // Return the RID-agnostic group
            return groups.GetDefaultAssets();
        }
    }
}
