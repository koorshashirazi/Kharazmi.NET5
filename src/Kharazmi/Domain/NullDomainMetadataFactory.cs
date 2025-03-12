using System;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;

namespace Kharazmi.Domain
{
    internal class NullDomainMetadataFactory : IDomainMetadataFactory, INullInstance
    {
        public void Dispose()
        {
        }

        public DomainMetadata GetCurrent => DomainMetadata.Empty;

        public void Create(string domainId, string? domainIdHeader = null)
        {
        }

        public void CreateFrom(DomainMetadata context)
        {
        }

        public void UpdateCurrent(Action<DomainMetadata> context)
        {
        }
    }
}