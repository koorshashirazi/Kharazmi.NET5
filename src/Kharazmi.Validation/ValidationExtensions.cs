#region

using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using Kharazmi.BuilderExtensions;
using Kharazmi.Extensions;
using Kharazmi.Validations;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Kharazmi.Validation
{
    public static class ValidationExtensions
    {
        /// <summary>
        /// To use model validation and domain validation
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static IConfigurePluginBuilder AddValidationConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            return builder;
        }

        internal static void WithValidationFactory(this IServiceCollection services,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            services.ReplaceService(typeof(IValidationHandler<>), typeof(FluentValidationHandler<>), serviceLifetime);
            services.ReplaceService<IValidatorFactory, ValidatorFactory>(serviceLifetime);
        }

        internal static void WithScanValidators(this IServiceCollection services,
            IEnumerable<Assembly>? assemblies = null, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            assemblies ??= new[] {Assembly.GetCallingAssembly()};
            AssemblyScanner.FindValidatorsInAssemblies(assemblies)
                .ForEach(result => services.AddScanResult(result, serviceLifetime));
        }

        private static void AddScanResult(this IServiceCollection services,
            AssemblyScanner.AssemblyScanResult result,
            ServiceLifetime serviceLifetime)
        {
            var skipAttribute = result.ValidatorType.GetCustomAttribute<SkipValidatorAttribute>();
            if (skipAttribute != null)
            {
                return;
            }

            var attribute = result.ValidatorType.GetCustomAttribute<EnableValidatorAttribute>();
            if (attribute != null && !attribute.Enable)
            {
                return;
            }

            //Register as interface
            services.AddService(
                result.InterfaceType,
                result.ValidatorType,
                serviceLifetime);

            //Register as self
            services.AddService(
                result.ValidatorType,
                result.ValidatorType,
                serviceLifetime);
        }
    }
}