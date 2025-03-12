using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Domain
{
    public interface IDomainIdProvider
    {
        DomainId GenerateDomainId();
    }
}