using Kharazmi.Common.ValueObjects;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Domain
{
    [Singleton]
    internal class DomainIdProvider : IDomainIdProvider
    {
        private readonly ILogger<DomainIdProvider> _logger;

        public DomainIdProvider(ILogger<DomainIdProvider>? logger)
        {
            _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<DomainIdProvider>();
        }

        public DomainId GenerateDomainId()
        {
            var domainId = DomainId.New;
            _logger.LogTrace(MessageTemplate.DomainMetadataGeneratedDomainIdUsingProvider,
                MessageEventName.DomainMetadata, nameof(DomainIdProvider), domainId.Value, GetType());
            return domainId;
        }
    }
}