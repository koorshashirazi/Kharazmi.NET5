using Kharazmi.Common.Metadata;
using Kharazmi.Domain;
using Kharazmi.Hooks;

namespace Kharazmi.Metadata
{
    internal class DomainIdDomainMetadataHook : IDomainMetadataHook
    {
        private readonly IDomainIdProvider _domainIdProvider;

        public DomainIdDomainMetadataHook(IDomainIdProvider domainIdProvider)
           => _domainIdProvider = domainIdProvider;
        
        public DomainMetadata Invoke(DomainMetadata domainMetadata)
        {
            domainMetadata.SetDomainId(_domainIdProvider.GenerateDomainId());
            return domainMetadata;
        }
    }
}