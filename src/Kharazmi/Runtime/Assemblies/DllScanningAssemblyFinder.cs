using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kharazmi.Extensions;

namespace Kharazmi.Runtime.Assemblies
{
    sealed class DllScanningAssemblyFinder : AssemblyFinder
    {
        public override IReadOnlyList<AssemblyName> FindAssembliesContainingName(string nameToFind)
        {
            var probeDirs = new List<string>();

            if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory))
            {
                probeDirs.Add(AppDomain.CurrentDomain.BaseDirectory);
            }
            else
            {
                var localAssemblyLocation = Path.GetDirectoryName(typeof(AssemblyFinder).Assembly.Location);
                if (localAssemblyLocation.IsNotEmpty())
                {
                    probeDirs.Add(localAssemblyLocation);
                }
            }

            var query = from probeDir in probeDirs
                where Directory.Exists(probeDir)
                from outputAssemblyPath in Directory.GetFiles(probeDir, "*.dll")
                let assemblyFileName = Path.GetFileNameWithoutExtension(outputAssemblyPath)
                where IsCaseInsensitiveMatch(assemblyFileName, nameToFind)
                let assemblyName = TryGetAssemblyNameFrom(outputAssemblyPath)
                where assemblyName is not null
                select assemblyName;

            return query.ToList().AsReadOnly();
        }

        private static AssemblyName? TryGetAssemblyNameFrom(string path)
        {
            try
            {
                return AssemblyName.GetAssemblyName(path);
            }
            catch (BadImageFormatException)
            {
                return null;
            }
        }
    }
}