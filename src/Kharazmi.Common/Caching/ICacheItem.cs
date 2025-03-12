using System;
using Newtonsoft.Json;

namespace Kharazmi.Common.Caching
{
    public interface ICacheItem
    {
        [JsonProperty] string? CacheType { get; }
        [JsonProperty] string CacheKey { get; }
        [JsonIgnore] TimeSpan? AbsoluteExpire { get; set; }
        ICacheItem BuildCacheKey();
    }
}