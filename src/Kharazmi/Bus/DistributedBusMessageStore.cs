using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Bus
{
    internal class DistributedBusMessageStore : IBusMessageStore, IMustBeInstance, IShouldBeSingleton
    {
        private readonly IDistributedCacheManager _cacheManager;

        public DistributedBusMessageStore(IDistributedCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public Task<Result<bool>> IsSetAsync(string messageId, string? nameSpace = null,
            CancellationToken token = default)
        {
            var cacheItem = new BusMessageStored(messageId, nameSpace).BuildCacheKey();
            return _cacheManager.ExistAsync(cacheItem.CacheKey, token);
        }

        public Task<IEnumerable<BusMessageStored>> GetAsync(int maxCount = 1000,
            CancellationToken token = default) => _cacheManager.GetAllAsync<BusMessageStored>("*", maxCount, token);

        public Task<Result> TryAddOrUpdateAsync(string messageId, string? nameSpace = null, TimeSpan? expireAt = null,
            CancellationToken token = default)
        {
            var cacheItem = new BusMessageStored(messageId, nameSpace);
            cacheItem.UpdateDateTime();
            return _cacheManager.AddOrUpdateAsync(new[] {cacheItem}, expireAt, token);
        }

        public Task<Result> TryRemoveAsync(string messageId, string? nameSpace = null,
            CancellationToken token = default)
        {
            var cacheItem = new BusMessageStored(messageId, nameSpace);
            return _cacheManager.RemoveAsync(new[] {cacheItem}, token);
        }
    }
}