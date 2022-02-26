#region

using System;

#endregion

namespace Regul.Base.Other
{
    public static class RegulPaths
    {
        public static string Themes => AppDomain.CurrentDomain.BaseDirectory + "Themes";
        public static string Modules => AppDomain.CurrentDomain.BaseDirectory + "Modules";
        public static string Cache => AppDomain.CurrentDomain.BaseDirectory + "Cache";
    }
}