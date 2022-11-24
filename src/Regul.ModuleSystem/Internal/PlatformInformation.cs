using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Regul.ModuleSystem.Internal;

internal class PlatformInformation
{
    public static readonly string[] NativeLibraryExtensions;
    public static readonly string[] NativeLibraryPrefixes;

    public static readonly string[] ManagedAssemblyExtensions =
    {
        ".dll",
        ".ni.dll",
        ".exe",
        ".ni.exe"
    };

    static PlatformInformation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            NativeLibraryPrefixes = new[] { "" };
            NativeLibraryExtensions = new[] { ".dll" };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            NativeLibraryPrefixes = new[] { "", "lib" };
            NativeLibraryExtensions = new[] { ".dylib" };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            NativeLibraryPrefixes = new[] { "", "lib" };
            NativeLibraryExtensions = new[] { ".so", ".so.1" };
        }
        else
        {
            #if DEBUG
            Debug.Fail("Unknown OS type");
            #endif
            NativeLibraryPrefixes = Array.Empty<string>();
            NativeLibraryExtensions = Array.Empty<string>();
        }
    }
}