using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Options;

namespace Kharazmi.Dependency
{
    public class ImplementationServiceTypeRegistry
    {
        private readonly HashSet<RuleOption> _rules;

        private static readonly Lazy<ImplementationServiceTypeRegistry> Lazy = new(() => new ImplementationServiceTypeRegistry());

        public static ImplementationServiceTypeRegistry Instance => Lazy.Value;
        
        private ImplementationServiceTypeRegistry()
        {
            _rules = new(new RuleOptionEqual());
        }

        public bool HasRule(Type implementationType)
        {
            var result = _rules.FirstOrDefault(x => x.ImplementationType == implementationType);
            return result.Condition;
        }

        public bool HasRule<TService>() where TService : class
            => HasRule(typeof(TService));

        public void AddRule(RuleOption rule) => _rules.Add(rule);
    }
}