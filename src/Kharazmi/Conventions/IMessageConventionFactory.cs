using System;
using System.Collections.Generic;
using Kharazmi.Dependency;

namespace Kharazmi.Conventions
{
    /// <summary>TryGetOrAdd a MessageConventions</summary>
    public interface IMessageConventionFactory : IShouldBeSingleton, IMustBeInstance
    {
        IReadOnlyDictionary<Type, IMessageConventions> GetAll();

        /// <summary>_</summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        IMessageConventions TryGetOrAdd(Type messageType);

        void Clear();
    }
}