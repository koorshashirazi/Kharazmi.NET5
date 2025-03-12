using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.Hooks;
using Kharazmi.Metadata;
using Kharazmi.Mvc.Hooks;
using Kharazmi.Options.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class DomainMetadataConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(DomainMetadataConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IDomainMetadataHook, HttpRequestDomainMetadataHook, NullDomainMetadataHook,
                DomainOption>((_, op) => op.UseDomainMetadata && op.DomainMetadataOption.UseHttpMetadata);
            
            services.RegisterWithFactory<IDomainMetadataHook, UserClaimsDomainMetadataHook, NullDomainMetadataHook,
                DomainOption>((_, op) => op.UseDomainMetadata && op.DomainMetadataOption.UseUserClaimsMetadata);

            services.RegisterWithFactory<IDomainMetadataHook, UserDomainMetadataHook, NullDomainMetadataHook,
                DomainOption>((_, op) => op.UseDomainMetadata && op.DomainMetadataOption.UseUserMetadata);
        }
    }
}