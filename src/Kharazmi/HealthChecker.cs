using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options;
using Kharazmi.Options.HealthCheck;
using Kharazmi.Threading;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi
{
    internal class HealthChecker : IHealthChecker
    {
        private readonly ILogger<HealthChecker>? _logger;
        private readonly ISettingProvider _settingProvider;
        private readonly IServiceProvider _serviceResolver;

        public HealthChecker(
            ILogger<HealthChecker>? logger,
            ISettingProvider settingProvider,
            IServiceProvider serviceResolver)
        {
            _logger = logger;
            _settingProvider = settingProvider;
            _serviceResolver = serviceResolver;
        }

        public IEnumerable<string> GetUnHealthOptions<TOption>() where TOption : class, IOptions
        {
            var option = _settingProvider.Get<TOption>();
            if (option.Enable == false)
                return Enumerable.Empty<string>();

            option.Validate();
            if (option.IsValid() == false)
            {
                foreach (var result in option.ValidationResults())
                {
                    _logger?.LogError(MessageTemplate.OptionValidation, MessageEventName.OptionsValidation,
                        nameof(HealthChecker), typeof(TOption).Name, result.ErrorMessage);
                }

                return new[] {typeof(TOption).Name};
            }

            List<string> unhealthiest = new();
            var optionKey = OptionHealthCheck(option);

            if (optionKey.IsNotEmpty())
                unhealthiest.Add(optionKey);

            if (option is IHaveNestedHealthCheck nestedHealthCheck)
            {
                foreach (var nestedOption in nestedHealthCheck.GetNestedOption())
                {
                    optionKey = NestedHealthCheck(nestedOption);
                    if (optionKey.IsNotEmpty())
                        unhealthiest.Add(optionKey);
                }
            }

            if (!(option is IChildOptions childOptions))
                return unhealthiest;

            foreach (var childOption in childOptions.GetChildOptions(childOptions.GetChildType()))
            {
                optionKey = ChildHealthCheck(childOption, childOptions.GetChildType());
                if (optionKey.IsNotEmpty())
                    unhealthiest.Add(optionKey);
            }

            return unhealthiest;
        }


        private string? OptionHealthCheck<TOption>(TOption obj) where TOption : class, IOptions
        {
            HealthCheckOption? healthCheckOption = null;
            Type? services = null;

            if (obj is IHaveHealthCheckOption op)
            {
                healthCheckOption = op.HealthCheckOption;
                services = HealthServiceTypeRegistry.Instance.GetHealthServiceType<TOption>();
            }

            if (healthCheckOption.IsNull()) return null;
            if (services.IsNull()) return null;
            if (HealthCheck(services, healthCheckOption)) return null;

            return typeof(TOption).Name;
        }

        private string? NestedHealthCheck<TOption>(TOption option) where TOption : class, INestedOption
        {
            HealthCheckOption? healthCheckOption = null;
            Type? services = null;

            option.Validate();
            if (option.IsValid() == false)
            {
                foreach (var result in option.ValidationResults())
                {
                    _logger?.LogError(MessageTemplate.OptionValidation, MessageEventName.OptionsValidation,
                        nameof(HealthChecker), typeof(TOption).Name, result.ErrorMessage);
                }

                return typeof(TOption).Name;
            }

            if (option is IHaveHealthCheckOption op)
            {
                healthCheckOption = op.HealthCheckOption;
                services = HealthServiceTypeRegistry.Instance.GetHealthServiceType(op.GetType());
            }

            if (healthCheckOption.IsNull()) return null;
            if (services.IsNull()) return null;
            return HealthCheck(services, healthCheckOption) ? null : option.GetType().Name;
        }

        private string? ChildHealthCheck<TOption>(TOption option, Type typeOption) where TOption : class, IChildOption
        {
            HealthCheckOption? healthCheckOption = null;
            Type? services = null;

            option.Validate();
            if (option.IsValid() == false)
            {
                foreach (var result in option.ValidationResults())
                {
                    _logger?.LogError(MessageTemplate.OptionValidation, MessageEventName.OptionsValidation,
                        nameof(HealthChecker), typeof(TOption).Name, result.ErrorMessage);
                }

                return option.OptionKey;
            }

            if (option is IHaveHealthCheckOption op)
            {
                healthCheckOption = op.HealthCheckOption;
                services = HealthServiceTypeRegistry.Instance.GetHealthServiceType(typeOption);
            }

            if (healthCheckOption.IsNull()) return null;

            if (services.IsNull()) return null;

            return HealthCheck(services, healthCheckOption) ? null : option.OptionKey;
        }

        private bool HealthCheck(Type services, HealthCheckOption option)
        {
            var checker = _serviceResolver.GetService(services);
            if (checker is not IHealthCheck healthCheck) return true;

            var result = AsyncHelper.RunSync(() => healthCheck.CheckHealthAsync(new HealthCheckContext
            {
                Registration = new HealthCheckRegistration(option.Name, healthCheck, HealthStatus.Unhealthy,
                    Enumerable.Empty<string>())
            }));
            if (result.Status == HealthStatus.Healthy)
                return true;

            _logger?.LogError(MessageTemplate.HealthCheckResult, MessageEventName.HealthCheck,
                nameof(HealthChecker), result.Description);
            return false;
        }
    }
}