using System;
using System.Collections.Generic;
using Kharazmi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kharazmi.HealthChecks
{
    public class HealthServiceTypeRegistry
    {
        private readonly Dictionary<Type, Type> _healthTypes;

        private static readonly Lazy<HealthServiceTypeRegistry> Lazy = new(() => new HealthServiceTypeRegistry());

        public static HealthServiceTypeRegistry Instance => Lazy.Value;

        private HealthServiceTypeRegistry()
        {
            _healthTypes = new Dictionary<Type, Type>();
        }

        public Type? GetHealthServiceType<TOption>() where TOption : class, IOptions
        {
            _healthTypes.TryGetValue(typeof(TOption), out var value);
            return value;
        }

        public Type? GetHealthServiceType(Type typeOption)
        {
            _healthTypes.TryGetValue(typeOption, out var value);
            return value;
        }

        public void RegisterForOption<TOption, TService>()
            where TOption : class, IOptions
            where TService : class, IHealthCheck
            => _healthTypes.TryAdd(typeof(TOption), typeof(TService));
        
        public void RegisterForNestedOption<TOption, TService>()
            where TOption : class, INestedOption
            where TService : class, IHealthCheck
            => _healthTypes.TryAdd(typeof(TOption), typeof(TService));

        public void RegisterForChildOption<TChildOption, TService>() where TChildOption : class, IChildOption
            where TService : class, IHealthCheck
            => _healthTypes.TryAdd(typeof(TChildOption), typeof(TService));
    }
}