using System;
using System.Reflection;
using Kharazmi.BuilderExtensions;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Domain;
using Kharazmi.Validations;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Validation.ConfigurePlugins
{
    public class ValidationConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(ValidationConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.ReplaceService(typeof(IValidationHandler<>), typeof(NullValidationHandler<>),
                ServiceLifetime.Transient);

            var option = services.GetSettings().Get<DomainOption>();

            if (!option.UseDomainValidation) return;

            var validationAssembly =
                Type.GetType(option.AssemblyDomainValidation)?.Assembly ?? Assembly.GetEntryAssembly();
            if (validationAssembly.IsNull())
                throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(MessageEventName.Assembly,
                    "AssemblyDomainValidation", validationAssembly?.FullName!));

            var assemblies = new[] {validationAssembly};
            services.WithScanValidators(assemblies);
            services.WithValidationFactory();
        }
    }
}