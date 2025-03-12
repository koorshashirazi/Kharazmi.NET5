using Newtonsoft.Json;

namespace Kharazmi.Common.Events.Cache
{
    /// <summary>_</summary>
    public class DistributedCacheCleared : DomainEvent
    {
        /// <summary>_</summary>
        [JsonConstructor]
        public DistributedCacheCleared()
        {
        }
    }
}