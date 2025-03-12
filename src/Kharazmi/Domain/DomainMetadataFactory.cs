using System;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;

namespace Kharazmi.Domain
{
    [Transient]
    internal class DomainMetadataFactory : IDomainMetadataFactory
    {
        private readonly IDomainMetadataAccessor _metadata;

        public DomainMetadataFactory(ServiceFactory<IDomainMetadataAccessor> metadata)
            => _metadata = metadata.Instance();

        public DomainMetadata GetCurrent => _metadata.DomainMetadata;

        public void Create(string domainId, string? domainIdHeader)
            => CreateFrom(DomainMetadata.Empty.SetDomainId(DomainId.FromString(domainId)));

        public void CreateFrom(DomainMetadata context)
            => _metadata.DomainMetadata = context;


        public void UpdateCurrent(Action<DomainMetadata> context)
        {
            var domainContext = GetCurrent;
            context.Invoke(domainContext);
            _metadata.DomainMetadata = domainContext;
        }

        public void Dispose()
            => _metadata.DomainMetadata = DomainMetadata.Empty;
    }
}