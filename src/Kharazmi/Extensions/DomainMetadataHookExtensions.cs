using System.Collections.Generic;
using Kharazmi.Common.Metadata;
using Kharazmi.Hooks;

namespace Kharazmi.Extensions
{
    public static class DomainMetadataHookExtensions
    {
        public static DomainMetadata GetDomainMetadata(this IEnumerable<IDomainMetadataHook> metadata)
        {
            var domainMetadata = DomainMetadata.Empty;
            foreach (var domainMetadataHook in metadata)
            {
                var hook = domainMetadataHook.Invoke(domainMetadata);
                domainMetadata = hook;
            }

            return domainMetadata;
        }
    }
}