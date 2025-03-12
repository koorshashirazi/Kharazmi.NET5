using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Kharazmi.Runtime.Assemblies
{
    internal abstract class AssemblyFinder
    {
        public abstract IReadOnlyList<AssemblyName> FindAssembliesContainingName(string nameToFind);

        protected static bool IsCaseInsensitiveMatch(string? text, string textToFind)
        {
            return text != null && text.ToLowerInvariant().Contains(textToFind.ToLowerInvariant());
        }

        public static AssemblyFinder Auto()
        {
            try
            {
                if (Assembly.GetEntryAssembly() != null && DependencyContext.Default != null)
                {
                    return new DependencyContextAssemblyFinder(DependencyContext.Default);
                }
            }
            catch (NotSupportedException) when (typeof(object).Assembly.Location is "")
            {
            }

            return new DllScanningAssemblyFinder();
        }

        public static AssemblyFinder ForSource(AssemblyScanStrategy assemblyScanStrategy)
        {
            return assemblyScanStrategy switch
            {
                AssemblyScanStrategy.UseLoadedAssemblies => Auto(),
                AssemblyScanStrategy.AlwaysScanDllFiles => new DllScanningAssemblyFinder(),
                _ => throw new ArgumentOutOfRangeException(nameof(assemblyScanStrategy),
                    assemblyScanStrategy, null),
            };
        }

        public static AssemblyFinder ForDependencyContext(DependencyContext dependencyContext)
        {
            return new DependencyContextAssemblyFinder(dependencyContext);
        }
    }
}