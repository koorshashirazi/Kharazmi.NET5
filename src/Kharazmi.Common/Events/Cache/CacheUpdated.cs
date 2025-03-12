using Kharazmi.Common.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kharazmi.Common.Events.Cache
{
    /// <summary>_</summary>
    public class CacheUpdated : DomainEvent
    {
        /// <summary>_</summary>
        [JsonConstructor]
        public CacheUpdated(object cacheItem)
        {
            CacheItem = cacheItem;
            CacheItemType = cacheItem.GetType().AssemblyQualifiedName;
            
            if (cacheItem is JObject cacheObject)
                CacheKey = cacheObject.GetValue(nameof(ICacheItem.CacheKey))?.ToObject<string>();

            if (cacheItem is ICacheItem cache)
                CacheKey = cache.CacheKey;
        }
        
        /// <summary>_</summary>
        public string? CacheItemType { get;  }

        /// <summary>_</summary>
        public string? CacheKey { get; }

        /// <summary>_</summary>
        public object CacheItem { get; }
    }
}