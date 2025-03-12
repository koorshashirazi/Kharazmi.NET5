using System.Reflection;

namespace Kharazmi.Runtime.Assemblies
{
    public static class AssemblyHelper
    {
        public static AssemblyName? GetEntryAssemblyName()
        {
            return Assembly.GetEntryAssembly()?.GetName();
        }
    }
}