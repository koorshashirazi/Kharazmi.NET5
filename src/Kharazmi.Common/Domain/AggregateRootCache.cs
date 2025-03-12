using System;
using System.Data;
using Kharazmi.Common.Caching;
using Kharazmi.Common.Extensions;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

namespace Kharazmi.Common.Domain
{
    /// <summary> Implementation of {IAggregateRootCache}</summary>
    public abstract class AggregateRootCache<TKey> : AggregateRoot<TKey>, IAggregateRootCache<TKey>
        where TKey : IEquatable<TKey>, IComparable
    {
        /// <summary></summary>
        /// <param name="aggregateId"></param>
        protected AggregateRootCache(IIdentity<TKey> aggregateId) : base(aggregateId)
        {
            CacheKey = $"{AggregateName}:{Id}";
            CacheType = GetType().AssemblyQualifiedName ??
                        GetType().Assembly.FullName ?? throw new NoNullAllowedException(nameof(CacheType));
        }


        /// <summary>_</summary>
        [JsonProperty]
        public string CacheType { get; private set; }

        /// <summary>_</summary>
        [JsonProperty]
        public string CacheKey { get; private set; }

        /// <summary>_</summary>
        [JsonIgnore]
        public TimeSpan? AbsoluteExpire { get; set; }


        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected ICacheItem UpdateCacheKey(string value)
        {
            if (value.IsNotEmpty())
                CacheKey = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected ICacheItem UpdateAbsoluteExpire(TimeSpan value)
        {
            AbsoluteExpire = value;
            return this;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public virtual ICacheItem BuildCacheKey()
        {
            CacheKey = $"{AggregateName}:{Id}";
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract IAggregateRootCache GetFromCache(object? value);
    }
}