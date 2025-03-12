using System;
using System.Collections.Generic;
using Kharazmi.Conventions;
using Kharazmi.Dependency;

namespace Kharazmi.RabbitMq.Default
{
    internal class NullMessageConventionFactory: IMessageConventionFactory, INullInstance
    {
        public IReadOnlyDictionary<Type, IMessageConventions> GetAll()
            => new Dictionary<Type, IMessageConventions>();

        public IMessageConventions TryGetOrAdd(Type messageType)
            => new NullMessageConventions();

        public void Clear()
        {
        }
    }
}