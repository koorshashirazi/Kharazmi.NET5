using System.Data;
using System.Threading;
using Kharazmi.Common.Metadata;

namespace Kharazmi.Domain
{
    internal class DomainMetadataAccessor : IDomainMetadataAccessor
    {
        private static readonly AsyncLocal<DomainMetadata> Context = new();

        public DomainMetadataAccessor()
        {
        }

        public DomainMetadata DomainMetadata
        {
            get => Context.Value ?? DomainMetadata.Empty;
            set
            {
                if (value is null)
                    throw new NoNullAllowedException(
                        "Domain context accessor can't access a null value for [DomainContext]");
                Context.Value = value;
            }
        }
    }
}