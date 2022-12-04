using System;
using System.IO;

namespace Regul.Other;

public static class RegulDirectories
{
    public static readonly string Modules = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
    public static readonly string Cache = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
}