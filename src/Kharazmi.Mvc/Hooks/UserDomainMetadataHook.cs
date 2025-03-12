using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Hooks;
using Kharazmi.Http;

namespace Kharazmi.Mvc.Hooks
{
    internal class UserDomainMetadataHook : IDomainMetadataHook
    {
        private readonly IUserContextAccessor _userHttpContext;

        public UserDomainMetadataHook(ServiceFactory<IUserContextAccessor> factory)
            => _userHttpContext = factory.Instance();

        public DomainMetadata Invoke(DomainMetadata domainMetadata)
        {
            if (_userHttpContext.UserId.HasValue && _userHttpContext.UserId.Value.IsNotEmpty())
                domainMetadata.SetUserId(new UserId(_userHttpContext.UserId.Value));

            // If you need more claims, you can add domainMetadata here
            return domainMetadata;
        }
    }
}