using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.HealthCheck
{
    public class HealthUiWebHookOption : NestedOption
    {
        public HealthUiWebHookOption()
        {
        }

        public string? Name { get; set; }

        public string? Uri { get; set; }

        public string? Payload { get; set; }

        public string? RestoredPayload { get; set; }

        public override void Validate()
        {
            if (!Uri.IsNull() && Uri.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthEndpointOption), nameof(Uri)));
        }
    }
}