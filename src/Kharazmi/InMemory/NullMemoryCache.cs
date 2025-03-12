using Kharazmi.Dependency;
using Microsoft.Extensions.Caching.Memory;

namespace Kharazmi.InMemory
{
    internal class NullMemoryCache : IMemoryCache, INullInstance, IShouldBeSingleton, IMustBeInstance
    {
        public void Dispose()
        {
        }

        public bool TryGetValue(object key, out object value)
        {
            value = default!;
            return true;
        }

        public ICacheEntry CreateEntry(object key)
            => default!;

        public void Remove(object key)
        {
        }
    }
}