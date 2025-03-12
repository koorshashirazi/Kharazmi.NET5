using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;

namespace Kharazmi.Domain
{
    internal class NullDomainMetadataAccessor : IDomainMetadataAccessor, INullInstance
    {
        public DomainMetadata? DomainMetadata
        {
#pragma warning disable 8766
            get => DomainMetadata.Empty;
#pragma warning restore 8766
            set
            {
            }
        }
    }
}