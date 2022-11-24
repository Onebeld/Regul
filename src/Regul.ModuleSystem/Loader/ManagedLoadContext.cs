using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Regul.ModuleSystem.Internal;
using Regul.ModuleSystem.LibraryModel;

namespace Regul.ModuleSystem.Loader;

public class ManagedLoadContext
{
    private readonly IReadOnlyCollection<string> _additionalProbingPaths;
    private readonly string _basePath;
    private readonly ICollection<string> _defaultAssemblies;
    private readonly bool _lazyLoadReferences;
    private readonly bool _loadInMemory;
    private readonly string _mainAssemblyPath;
    private readonly IReadOnlyDictionary<string, ManagedLibrary> _managedAssemblies;
    private readonly bool _preferDefaultLoadContext;
    private readonly IReadOnlyCollection<string> _privateAssemblies;
    private readonly string[] _resourceRoots;
    private readonly string _unmanagedDllShadowCopyDirectoryPath;

    public ManagedLoadContext(string mainAssemblyPath,
        IReadOnlyDictionary<string, ManagedLibrary> managedAssemblies,
        IReadOnlyCollection<string> privateAssemblies,
        IReadOnlyCollection<string> defaultAssemblies,
        IReadOnlyCollection<string> additionalProbingPaths,
        IReadOnlyCollection<string> resourceProbingPaths,
        bool preferDefaultLoadContext,
        bool lazyLoadReferences,
        bool loadInMemory)
    {
        if (resourceProbingPaths == null) throw new ArgumentNullException(nameof(resourceProbingPaths));

        _mainAssemblyPath = mainAssemblyPath ?? throw new ArgumentNullException(nameof(mainAssemblyPath));
        _basePath = Path.GetDirectoryName(mainAssemblyPath) ??
                    throw new ArgumentException(nameof(mainAssemblyPath));
        _managedAssemblies = managedAssemblies ?? throw new ArgumentNullException(nameof(managedAssemblies));
        _privateAssemblies = privateAssemblies ?? throw new ArgumentNullException(nameof(privateAssemblies));
        _defaultAssemblies = defaultAssemblies != null
            ? defaultAssemblies.ToList()
            : throw new ArgumentNullException(nameof(defaultAssemblies));
        _additionalProbingPaths =
            additionalProbingPaths ?? throw new ArgumentNullException(nameof(additionalProbingPaths));
        _preferDefaultLoadContext = preferDefaultLoadContext;
        _loadInMemory = loadInMemory;
        _lazyLoadReferences = lazyLoadReferences;

        _resourceRoots = new[]
            {
                _basePath
            }
            .Concat(resourceProbingPaths)
            .ToArray();
        _unmanagedDllShadowCopyDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }

    /// <summary>
    ///     Load an assembly.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public Assembly Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == null)
            // not sure how to handle this case. It's technically possible.
            return null;

        if ((_preferDefaultLoadContext || _defaultAssemblies.Contains(assemblyName.Name)) &&
            !_privateAssemblies.Contains(assemblyName.Name))
            // If default context is preferred, check first for types in the default context unless the dependency has been declared as private
            try
            {
                Assembly defaultAssembly = Assembly.Load(assemblyName);
                if (defaultAssembly != null)
                {
                    // Add referenced assemblies to the list of default assemblies.
                    // This is basically lazy loading
                    if (_lazyLoadReferences)
                        foreach (AssemblyName reference in defaultAssembly.GetReferencedAssemblies())
                            if (reference.Name != null && !_defaultAssemblies.Contains(reference.Name))
                                _defaultAssemblies.Add(reference.Name);

                    // Older versions used to return null here such that returned assembly would be resolved from the default ALC.
                    // However, with the addition of custom default ALCs, the Default ALC may not be the user's chosen ALC when
                    // this context was built. As such, we simply return the Assembly from the user's chosen default load context.
                    return defaultAssembly;
                }
            }
            catch
            {
                // Swallow errors in loading from the default context
            }

        if (!string.IsNullOrEmpty(assemblyName.CultureName) && !string.Equals("neutral", assemblyName.CultureName))
        {
            foreach (string resourceRoot in _resourceRoots)
            {
                string resourcePath =
                    Path.Combine(resourceRoot, assemblyName.CultureName, assemblyName.Name + ".dll");
                if (File.Exists(resourcePath)) return LoadAssemblyFromFilePath(resourcePath);
            }

            return null;
        }

        if (_managedAssemblies.TryGetValue(assemblyName.Name, out ManagedLibrary library) && library != null)
        {
            if (SearchForLibrary(library, out string path) && path != null) return LoadAssemblyFromFilePath(path);
        }
        else
        {
            // if an assembly was not listed in the list of known assemblies,
            // fallback to the load context base directory
            string dllName = assemblyName.Name + ".dll";
            foreach (string probingPath in _additionalProbingPaths.Prepend(_basePath))
            {
                string localFile = Path.Combine(probingPath, dllName);
                if (File.Exists(localFile)) return LoadAssemblyFromFilePath(localFile);
            }
        }

        return null;
    }

    public Assembly LoadAssemblyFromFilePath(string path)
    {
        if (!_loadInMemory) return Assembly.LoadFrom(path);

        using (FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (MemoryStream ms = new())
            {
                file.CopyTo(ms);
                string pdbPath = Path.ChangeExtension(path, ".pdb");
                if (File.Exists(pdbPath))
                    using (FileStream pdbFile = File.Open(pdbPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (MemoryStream ms1 = new())
                        {
                            pdbFile.CopyTo(ms1);
                            return Assembly.Load(ms.ToArray(), ms1.ToArray());
                        }
                    }

                return Assembly.Load(ms.ToArray());
            }
        }
    }

    private bool SearchForLibrary(ManagedLibrary library, out string path)
    {
        // 1. Check for in _basePath + app local path
        string localFile = Path.Combine(_basePath, library.AppLocalPath);
        if (File.Exists(localFile))
        {
            path = localFile;
            return true;
        }

        // 2. Search additional probing paths
        foreach (string searchPath in _additionalProbingPaths)
        {
            string candidate = Path.Combine(searchPath, library.AdditionalProbingPath);
            if (File.Exists(candidate))
            {
                path = candidate;
                return true;
            }
        }

        // 3. Search in base path
        foreach (string ext in PlatformInformation.ManagedAssemblyExtensions)
        {
            string local = Path.Combine(_basePath, library.Name.Name + ext);
            if (File.Exists(local))
            {
                path = local;
                return true;
            }
        }

        path = null;
        return false;
    }

    private string CreateShadowCopy(string dllPath)
    {
        Directory.CreateDirectory(_unmanagedDllShadowCopyDirectoryPath);

        string dllFileName = Path.GetFileName(dllPath);
        string shadowCopyPath = Path.Combine(_unmanagedDllShadowCopyDirectoryPath, dllFileName);

        if (!File.Exists(shadowCopyPath)) File.Copy(dllPath, shadowCopyPath);

        return shadowCopyPath;
    }
}