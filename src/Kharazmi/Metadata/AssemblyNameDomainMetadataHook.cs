using Kharazmi.Common.Metadata;
using Kharazmi.Hooks;
using Kharazmi.Runtime.Assemblies;

namespace Kharazmi.Metadata
{
    internal class AssemblyNameDomainMetadataHook : IDomainMetadataHook
    {
        public DomainMetadata Invoke(DomainMetadata domainMetadata)
        {
            var domainAssemblyName = AssemblyHelper.GetEntryAssemblyName();
            domainMetadata.SetDomainAssemblyName(domainAssemblyName);
            return domainMetadata;
        }
    }
}