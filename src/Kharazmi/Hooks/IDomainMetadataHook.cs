using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;

namespace Kharazmi.Hooks
{
    public interface IDomainMetadataHook : IMustBeInstance
    {
        DomainMetadata Invoke(DomainMetadata domainMetadata);
    }
}