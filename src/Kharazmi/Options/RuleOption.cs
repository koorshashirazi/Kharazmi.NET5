using System;
using System.Collections.Generic;

namespace Kharazmi.Options
{
    internal class RuleOptionEqual : IEqualityComparer<RuleOption>
    {
        public bool Equals(RuleOption x, RuleOption y)
        {
            return x.ServiceType == y.ServiceType &&
                   x.ImplementationType == y.ImplementationType;
        }

        public int GetHashCode(RuleOption obj)
        {
            return HashCode.Combine(obj.ServiceType, obj.ImplementationType);
        }
    }

    public readonly struct RuleOption
    {
        public static RuleOption For(Type serviceType, Type implementationType, bool condition)
            => new(serviceType, implementationType, condition);

        public static RuleOption For<TService, TImplementation>(bool condition)
            where TService : class where TImplementation : class, TService
            => new(typeof(TService), typeof(TImplementation), condition);

        private RuleOption(Type serviceType, Type implementationType, bool condition)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Condition = condition;
        }

        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public bool Condition { get; }
    }
}