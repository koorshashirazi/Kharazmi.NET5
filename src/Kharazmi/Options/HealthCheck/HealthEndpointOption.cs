using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.HealthCheck
{
    public class HealthEndpointOption : NestedOption
    {
        public HealthEndpointOption()
        {
            Name = "Health-Check";
            Uri = "/health";
        }

        public string Name { get; set; }
        public string Uri { get; set; }


        public override void Validate()
        {
            if (Uri.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthEndpointOption), nameof(Uri)));
        }
    }
}