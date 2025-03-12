using Kharazmi.Common.Caching;

namespace Kharazmi.Caching
{
    internal sealed class ValueKeyCacheItem : CacheItem
    {
        public ValueKeyCacheItem(string key)
        {
            UpdateCacheItemKey(key);
        }
    }
}