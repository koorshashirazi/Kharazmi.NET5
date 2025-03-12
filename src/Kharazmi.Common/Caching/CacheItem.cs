using System;
using System.Data;
using Kharazmi.Common.Extensions;
using Newtonsoft.Json;

namespace Kharazmi.Common.Caching
{
    /// <summary>_</summary>
    public abstract class CacheItem : ICacheItem
    {
        /// <summary>_</summary>
        [JsonConstructor]
        protected CacheItem()
        {
            CacheKey = GetType().UniqueTypeName();
        }

        /// <summary>_</summary>
        [JsonProperty]
        public string? CacheType => GetType().AssemblyQualifiedName;

        /// <summary>_</summary>
        [JsonProperty]
        public string CacheKey { get; private set; }

        /// <summary>_</summary>
        [JsonIgnore]
        public TimeSpan? AbsoluteExpire { get; set; }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual CacheItem UpdateCacheItemKey(string value)
        {
            if (value.IsEmpty())
                throw new NoNullAllowedException(
                    $"CacheItem can't accept empty value for cache key with type {CacheType}");
            CacheKey = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected CacheItem UpdateAbsoluteExpire(TimeSpan value)
        {
            AbsoluteExpire = value;
            return this;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public virtual ICacheItem BuildCacheKey()
        {
            return this;
        }
    }
}