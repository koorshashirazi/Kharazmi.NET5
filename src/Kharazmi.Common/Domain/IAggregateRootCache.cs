using System;
using Kharazmi.Common.Caching;

namespace Kharazmi.Common.Domain
{
    /// <summary>_</summary>
    public interface IAggregateRootCache : IAggregateRoot, ICacheItem
    {
        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IAggregateRootCache? GetFromCache(object? value);
    }

    /// <summary>_</summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IAggregateRootCache<TKey> : IAggregateRoot<TKey>, IAggregateRootCache
        where TKey : IEquatable<TKey>, IComparable
    {
    }
}