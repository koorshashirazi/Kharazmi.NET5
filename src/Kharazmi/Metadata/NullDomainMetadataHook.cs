using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Hooks;

namespace Kharazmi.Metadata
{
    public class NullDomainMetadataHook : IDomainMetadataHook, INullInstance
    {
        public DomainMetadata Invoke(DomainMetadata domainMetadata)
            => domainMetadata;
    }
}