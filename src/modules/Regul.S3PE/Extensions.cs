using System.Reflection;
using Regul.S3PI.Resources;

namespace Regul.S3PE
{
    public static class Extensions
    {
        public static string GetAssemblyName(this ScriptResource scriptResource)
        {
            try
            {
                byte[] data = new byte[scriptResource.Assembly.BaseStream.Length];
                scriptResource.Assembly.BaseStream.Read(data, 0, data.Length);
                Assembly assembly = Assembly.Load(data);

                return assembly.FullName.Split(',')[0] + ".dll";
            }
            catch { }

            return "*.dll";
        }
    }
}