using Kharazmi.Extensions;

namespace Kharazmi.Options.Domain
{
    public class DomainMetadataOption : NestedOption
    {
        private DomainIdMetadataOption _domainIdMetadataOption;

        public DomainMetadataOption()
        {
            _domainIdMetadataOption = new DomainIdMetadataOption();
        }

        public bool UseAssemblyName { get; set; }
        public bool UseMachineName { get; set; }
        public bool UseUserMetadata { get; set; }
        public bool UseUserClaimsMetadata { get; set; }
        public bool UseHttpMetadata { get; set; }
        public bool UseDomainId { get; set; }

        public DomainIdMetadataOption DomainIdMetadataOption
        {
            get => _domainIdMetadataOption;
            set
            {
                if(value.IsNull()) return;
                _domainIdMetadataOption = value;
            }
        }


        public override void Validate()
        {
            if (!UseDomainId) return;
            DomainIdMetadataOption.Validate();
        }
    }
}