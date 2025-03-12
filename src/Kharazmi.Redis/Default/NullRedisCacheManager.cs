using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Functional;
using Kharazmi.Options.Redis;
using StackExchange.Redis;

#pragma warning disable 1591

namespace Kharazmi.Redis.Default
{
    public class NullRedisCacheManager : NullCacheManager, IRedisCacheManager
    {
        public NullRedisCacheManager()
        {
            RedisOption = new RedisDbOption();
        }

        public RedisDbOption RedisOption { get; }

        public IRedisCacheManager SetDatabaseTo(RedisDbOption option)
        {
            return this;
        }

        public IDatabase? Database { get; }

        public IPagedList<ICacheItem> GetCacheItems(IEnumerable<string> keys, int page = 0, int pageSize = 10)
            => new PagedList<ICacheItem>();

        public Task<IPagedList<ICacheItem>> GetCacheItemsAsync(IEnumerable<string> keys, int page = 0,
            int pageSize = 10,
            CancellationToken token = default) => Task.FromResult<IPagedList<ICacheItem>>(new PagedList<ICacheItem>(
            Enumerable.Empty<ICacheItem>(), page, pageSize, 0));

        public IPagedList<ICacheItem> GetCacheItems(string pattern = "*", int page = 0, int pageSize = 10)
            => new PagedList<ICacheItem>();

        public Task<IPagedList<ICacheItem>> GetCacheItemsAsync(string pattern = "*", int page = 0, int pageSize = 10,
            CancellationToken token = default)
            => Task.FromResult<IPagedList<ICacheItem>>(new PagedList<ICacheItem>(
                Enumerable.Empty<ICacheItem>(), page, pageSize, 0));

        public Task<Result> AddOrUpdateTransactionAsync<T>(T value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IRedisCacheManager"));

        public Task<Result> UpdateTransactionAsync<T>(string key, T value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IRedisCacheManager"));

        public Task<Result> RemoveTransactionAsync<T>(T[] value, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IRedisCacheManager"));

        public Task<Result> RemoveAllTransactionAsync(CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IRedisCacheManager"));
    }
}