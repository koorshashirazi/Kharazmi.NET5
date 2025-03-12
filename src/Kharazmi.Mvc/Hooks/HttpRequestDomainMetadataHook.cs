using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Hooks;
using Kharazmi.Http;
using Kharazmi.Mvc.Extensions;

namespace Kharazmi.Mvc.Hooks
{
    internal class HttpRequestDomainMetadataHook : IDomainMetadataHook
    {
        private readonly IUserContextAccessor _userHttpContext;

        public HttpRequestDomainMetadataHook(ServiceFactory<IUserContextAccessor> factory)
            => _userHttpContext = factory.Instance();

        public DomainMetadata Invoke(DomainMetadata domainMetadata)
        {
            if (_userHttpContext.TraceId.HasValue && _userHttpContext.TraceId.Value.IsNotEmpty())
                domainMetadata.SetTraceId(_userHttpContext.TraceId.Value);

            if (_userHttpContext.ConnectionId.HasValue && _userHttpContext.ConnectionId.Value.IsNotEmpty())
                domainMetadata.SetConnectionId(_userHttpContext.ConnectionId.Value);

            if (_userHttpContext.Request.HasValue && _userHttpContext.Request.Value.GetOrigin().IsNotEmpty())
                domainMetadata.SetOrigin(_userHttpContext.Request.Value.GetOrigin());

            if (_userHttpContext.RequestPath.HasValue && _userHttpContext.RequestPath.Value.IsNotEmpty())
                domainMetadata.SetRequestPath(_userHttpContext.RequestPath.Value);

            return domainMetadata;
        }
    }
}