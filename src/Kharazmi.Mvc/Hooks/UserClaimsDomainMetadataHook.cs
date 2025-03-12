using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Hooks;
using Kharazmi.Http;

namespace Kharazmi.Mvc.Hooks
{
    internal class UserClaimsDomainMetadataHook : IDomainMetadataHook
    {
        private readonly IUserContextAccessor _userHttpContext;

        public UserClaimsDomainMetadataHook(ServiceFactory<IUserContextAccessor> factory)
            => _userHttpContext = factory.Instance();

        public DomainMetadata Invoke(DomainMetadata domainMetadata)
        {
// TODO
            return domainMetadata;
        }
    }
}