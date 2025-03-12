using Kharazmi.Common.Metadata;
using Kharazmi.Hooks;
using Kharazmi.Runtime.Environments;

namespace Kharazmi.Metadata
{
    internal class MachineNameDomainMetadataHook : IDomainMetadataHook
    {
        public DomainMetadata Invoke(DomainMetadata domainMetadata)
        {
            var name = EnvironmentHelper.GetMachineName();
            domainMetadata.SetMachineName(name);
            return domainMetadata;
        }
    }
}