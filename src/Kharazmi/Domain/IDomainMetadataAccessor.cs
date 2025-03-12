using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;

namespace Kharazmi.Domain
{
    public interface IDomainMetadataAccessor : IShouldBeSingleton, IMustBeInstance
    {
        public DomainMetadata DomainMetadata { get; set; }
    }
}