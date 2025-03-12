using Newtonsoft.Json;

namespace Kharazmi.Common.Events.Cache
{
    /// <summary>_</summary>
    public class CacheCleared : DomainEvent
    {
        /// <summary>_</summary>
        [JsonConstructor]
        public CacheCleared()
        {
        }
    }
}