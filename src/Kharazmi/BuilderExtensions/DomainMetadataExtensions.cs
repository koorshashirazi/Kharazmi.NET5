using Kharazmi.Domain;
using Kharazmi.Extensions;
using Kharazmi.Options.Domain;
using Microsoft.AspNetCore.Builder;

namespace Kharazmi.BuilderExtensions
{
    public static class DomainMetadataExtensions
    {
        public static IApplicationBuilder UseCorrelationDomainMetadata(this IApplicationBuilder builder)
        {
            var settings = builder.ApplicationServices.GetSettings();
            var option = settings.Get<DomainOption>();
            if (option.IsNull()) return builder;
            return option.UseDomainMetadata == false
                ? builder
                : builder.UseMiddleware<CorrelationDomainMetadataMiddleware>();
        }
    }
}