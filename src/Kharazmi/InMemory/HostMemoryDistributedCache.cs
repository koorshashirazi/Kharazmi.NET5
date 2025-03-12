using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Collections;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kharazmi.InMemory
{
    internal class HostMemoryDistributedCache : IDistributedCacheExtended, IMustBeInstance, IShouldBeSingleton
    {
        private readonly IMemoryCache _memCache;
        private readonly ILogger<HostMemoryDistributedCache> _logger;
        private static readonly HashSet<string> KeyHolder = new();

        public HostMemoryDistributedCache(
            IOptions<MemoryCacheOptions> memOption,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HostMemoryDistributedCache>();
            _memCache = new MemoryCache(memOption);
        }

        public byte[] Get(string key)
        {
            key.NotEmpty(nameof(key));
            return (byte[]) _memCache.Get(key);
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            key.NotEmpty(nameof(key));
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            key.NotEmpty(nameof(key));
            value.NotNull(nameof(value));
            options.NotNull(nameof(DistributedCacheEntryOptions));

            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
                Size = value.Length
            };

            KeyHolder.Add(key);
            _memCache.Set(key, value, memoryCacheEntryOptions);

            _logger.LogTrace(MessageTemplate.SetCacheItem, MessageEventName.CacheItemAdded,
                nameof(HostMemoryDistributedCache), key);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            key.NotEmpty(nameof(key));
            value.NotNull(nameof(value));
            options.NotNull(nameof(DistributedCacheEntryOptions));

            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
            key.NotEmpty(nameof(key));
            _memCache.TryGetValue(key, out object value);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            key.NotEmpty(nameof(key));
            Refresh(key);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            key.NotEmpty(nameof(key));
            KeyHolder.Remove(key);
            _memCache.Remove(key);
            _logger.LogTrace(MessageTemplate.RemoveCacheItem, MessageEventName.CacheItemRemoved,
                nameof(HostMemoryDistributedCache), key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            key.NotEmpty(nameof(key));
            Remove(key);
            return Task.CompletedTask;
        }

        public IEnumerable<string> GetKeys(string pattern, int total = 1000)
        {
            return pattern == "*"
                ? KeyHolder.Take(total)
                : KeyHolder.Where(x => x.Contains(pattern)).Take(total);
        }

        public Task<IEnumerable<string>> GetKeysAsync(string pattern, int total = 1000,
            CancellationToken token = default)
            => Task.FromResult(GetKeys(pattern));

        public async Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100,
            int total = 1000, CancellationToken token = default) where T : class
        {
            var totalKeys = keys.ToArray();
            var tasks = totalKeys.PageBy(pageNumber, pageSize).Select(key => GetAsync(key, token));
            var result = await Task.WhenAll(tasks).ConfigureAwait(false);
            PagedList<T> pagedList = new() {TotalCount = totalKeys.Length};

            for (var index = 0; index < totalKeys.Length; index++)
            {
                var valueBytes = result[index];
                var value = valueBytes.FromBytes<T>();
                if (value is null) continue;
                pagedList.Add(value);
            }

            return pagedList;
        }

        public Task<Result> RemoveAsync(IEnumerable<string> keys, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (var key in keys)
                    Remove(key);

                return Task.FromResult(Result.Ok());
            }
            catch (Exception e)
            {
                return Task.FromResult(Result.Fail(e.Message));
            }
        }

        public Task<Result> RemoveAllAsync(CancellationToken token = default)
            => RemoveAsync(KeyHolder, token);
    }
}