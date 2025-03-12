using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kharazmi.Common.Events.Cache
{
    /// <summary>_</summary>
    public class CacheRemoved : DomainEvent
    {
      
        /// <summary>_</summary>
        [JsonConstructor]
        public CacheRemoved(IEnumerable<string> cacheKeys)
        {
            CacheKeys = cacheKeys;
        }

        /// <summary>_</summary>
        public IEnumerable<string> CacheKeys { get;  }

        /// <summary>_</summary>
        public string CacheKeysString => string.Join(",", CacheKeys);
    }
}