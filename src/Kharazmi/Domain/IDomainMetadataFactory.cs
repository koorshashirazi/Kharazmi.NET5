using System;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;

namespace Kharazmi.Domain
{
    public interface IDomainMetadataFactory : IMustBeInstance, IDisposable
    {
        DomainMetadata GetCurrent { get; }
        void Create(string domainId, string? domainIdHeader = null);
        void CreateFrom(DomainMetadata context);
        void UpdateCurrent(Action<DomainMetadata> context);
    }
}