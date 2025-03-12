using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Common.Events.Cache;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Dispatchers;
using Kharazmi.Domain;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Json;
using Kharazmi.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Caching
{
    internal class DistributedCacheManager : CacheManagerBase, IDistributedCacheManager
    {
        private readonly IDomainMetadataAccessor _domainMetadataAccessor;
        private readonly IDomainDispatcher _domainDispatcher;
        private readonly IDistributedCacheExtended _cache;

        public DistributedCacheManager(
            ServiceFactory<IDomainDispatcher> domainDispatcher,
            ServiceFactory<IDomainMetadataAccessor> domainMetadataAccessor,
            ServiceFactory<IDistributedCacheExtended> distributedCacheFactory,
            ISettingProvider settingProvider,
            ILoggerFactory loggerFactory) : base(settingProvider, loggerFactory)
        {
            _domainMetadataAccessor = domainMetadataAccessor.Instance();
            _domainDispatcher = domainDispatcher.Instance();
            _cache = distributedCacheFactory.Instance();
        }

        #region Query

        public Result<bool> Exist(string key)
            => AsyncHelper.RunSync(() => ExistAsync(key));

        public async Task<Result<bool>> ExistAsync(string key, CancellationToken token = default)
        {
            var value = await _cache.GetAsync(key, token);
            return Result.OkAs(value is not null);
        }

        public Result<bool> Exist<T>(T value) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => ExistAsync(value.BuildCacheKey().CacheKey));

        public Task<Result<bool>> ExistAsync<T>(T value, CancellationToken token = default) where T : class, ICacheItem
            => ExistAsync(value.BuildCacheKey().CacheKey, token);

        public Maybe<object> Find(string key)
            => AsyncHelper.RunSync(() => FindAsync(key));

        public async Task<Maybe<object>> FindAsync(string key, CancellationToken token = default)
            => await _cache.GetStringAsync(key, token);

        public Maybe<T> Find<T>(string key) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => FindAsync<T>(key));

        public async Task<Maybe<T>> FindAsync<T>(string key, CancellationToken token = default)
            where T : class, ICacheItem
        {
            var value = await _cache.GetStringAsync(key, token);
            return value.Deserialize<T>() ?? Maybe<T>.None;
        }

        public Maybe<T> Find<T>(T value) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => FindAsync(value));

        public Task<Maybe<T>> FindAsync<T>(T value, CancellationToken token = default)
            where T : class, ICacheItem
        {
            var key = value.BuildCacheKey().CacheKey;
            return FindAsync<T>(key, token);
        }

        public Maybe<T> FindBy<T>(Func<T, bool> predicate) where T : class, ICacheItem
        {
            var item = GetAll<T>().FirstOrDefault(predicate);
            return item ?? Maybe<T>.None;
        }

        public async Task<Maybe<T>> FindByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem
        {
            var items = await GetAllAsync<T>(token: token);
            var item = items.FirstOrDefault(predicate);
            return item ?? Maybe<T>.None;
        }

        public IEnumerable<string> GetKeys(string pattern = "*", int total = 1000) => _cache.GetKeys(pattern, total);

        public Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int total = 1000,
            CancellationToken token = default) => _cache.GetKeysAsync(pattern, total, token);

        public IEnumerable<T> GetAll<T>(string pattern = "*", int maxCount = 1000) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAllAsync<T>(pattern, maxCount));

        public async Task<IEnumerable<T>> GetAllAsync<T>(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem
        {
            var keys = await GetKeysAsync(pattern, maxCount, token);
            return await GetAsync<T>(keys, 0, maxCount, token);
        }

        public IPagedList<T> Get<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000)
            where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAsync<T>(pageNumber, pageSize, maxCount));

        public async Task<IPagedList<T>> GetAsync<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem
        {
            var keys = await GetKeysAsync("*", maxCount, token);
            return await GetAsync<T>(keys, pageNumber, pageSize, token);
        }

        public IPagedList<T> Get<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100)
            where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAsync<T>(keys, pageNumber, pageSize));

        public async Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100,
            CancellationToken token = default)
            where T : class, ICacheItem
        {
            var totalKey = keys.ToList();
            var pagedKey = totalKey.PageBy(pageNumber, pageSize);
            var tasks = await Task.WhenAll(pagedKey.Select(key => FindAsync<T>(key, token)));
            return new PagedList<T>(tasks.Where(x => x.HasValue).Select(x => x.Value), pageNumber, pageSize,
                totalKey.Count);
        }

        public IEnumerable<T> GetBy<T>(Func<T, bool> predicate) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetByAsync(predicate));

        public async Task<IEnumerable<T>> GetByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem
        {
            var items = await GetAsync<T>(0, 1000, 1000, token);
            return items.Where(predicate);
        }

        #endregion

        #region Commands

        public Result AddOrUpdate<T>(T[] value, TimeSpan? expiresIn = default) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => AddOrUpdateAsync(value, expiresIn));

        public async Task<Result> AddOrUpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
        {
            async Task<Result> UpdateTask(T cacheItem)
            {
                var cacheKey = cacheItem.BuildCacheKey().CacheKey;
                if (cacheKey.IsEmpty())
                {
                    Logger?.LogError("Invalid cache key with cache type {CacheType}", cacheItem.GetType().Name);
                    return Result.Fail($"Invalid cache key with cache type {cacheItem.GetType().Name}");
                }

                var result = await TryExecuteAsync(() =>
                    _cache.SetAsync(cacheKey, cacheItem.ToBytes(), BuildDistributedCacheEntryOptions(expiresIn),
                        token));

                if (result.Failed)
                {
                    Logger?.LogError("Execute of Operation {Operation} is failed", nameof(AddOrUpdateAsync));
                    return result;
                }

                var message = new DistributedCacheAdded(cacheItem);
                return await _domainDispatcher
                    .RaiseAsync(message, message.GetType(), _domainMetadataAccessor.DomainMetadata, token)
                    .ConfigureAwait(false);
            }

            var tasks = value.Select(UpdateTask);
            var results = await Task.WhenAll(tasks);

            return Result.Combine(results);
        }

        public Result Update<T>(T[] value, TimeSpan? expiresIn = default)
            where T : class, ICacheItem
            => AsyncHelper.RunSync(() => UpdateAsync(value, expiresIn));

        public async Task<Result> UpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default)
            where T : class, ICacheItem
        {
            async Task<Result> UpdateTask(T cacheItem)
            {
                var cacheKey = cacheItem.BuildCacheKey().CacheKey;

                if (cacheKey.IsEmpty())
                {
                    Logger?.LogError("Invalid cache key with cache type {CacheType}", cacheItem.CacheType);
                    return Result.Fail($"Invalid cache key with cache type {cacheItem.CacheType}");
                }

                var cache = await FindAsync<T>(cacheKey, token);

                if (!cache.HasValue)
                {
                    Logger?.LogError("Cache item is not found with cache type {CacheType} and cache key {CacheKey}",
                        cacheItem.CacheType, cacheKey);
                    return Result.Fail(
                        $"Cache item is not found with cache type {typeof(T).Name} and cache key {cacheKey}");
                }

                var result = await TryExecuteAsync(() =>
                    _cache.SetAsync(cacheKey, cacheItem.ToBytes(), BuildDistributedCacheEntryOptions(expiresIn),
                        token));

                if (result.Failed)
                {
                    Logger?.LogError("Execute of Operation {Operation} is failed", nameof(AddOrUpdateAsync));
                    return result;
                }

                var message = new DistributedCacheUpdated(cacheItem);
                return await _domainDispatcher
                    .RaiseAsync(message, message.GetType(), _domainMetadataAccessor.DomainMetadata, token)
                    .ConfigureAwait(false);
            }

            var tasks = value.Select(UpdateTask);
            var results = await Task.WhenAll(tasks);

            return Result.Combine(results);
        }

        public Result Remove<T>(T[] value) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => RemoveAsync(value));

        public Task<Result> RemoveAsync<T>(T[] value, CancellationToken token = default)
            where T : class, ICacheItem
        {
            return TryExecuteAsync(async () =>
            {
                var keys = value.Select(x => x.BuildCacheKey().CacheKey).ToList();

                var result = await _cache.RemoveAsync(keys, token);
                if (result.Failed)
                    throw new DistributedCacheException(result);

                var message = new DistributedCacheRemoved(keys);

                await _domainDispatcher.RaiseAsync(message, message.GetType(),
                    _domainMetadataAccessor.DomainMetadata, token).ConfigureAwait(false);
            });
        }

        public Result RemoveAll()
            => AsyncHelper.RunSync(() => RemoveAllAsync());

        public Task<Result> RemoveAllAsync(CancellationToken token = default)
        {
            return TryExecuteAsync(async () =>
            {
                var result = await _cache.RemoveAllAsync(token);

                if (result.Failed)
                    throw new DistributedCacheException(result);

                var message = new DistributedCacheCleared();
                await _domainDispatcher
                    .RaiseAsync(message, message.GetType(), _domainMetadataAccessor.DomainMetadata, token)
                    .ConfigureAwait(false);
            });
        }

        public Result RemoveBy(Func<string, bool> predicate)
            => AsyncHelper.RunSync(() => RemoveByAsync(predicate));

        public async Task<Result> RemoveByAsync(Func<string, bool> predicate, CancellationToken token = default)
        {
            var keys = await _cache.GetKeysAsync(token: token);
            keys = keys.Where(predicate);
            return await _cache.RemoveAsync(keys, token);
        }

        #endregion


        public void Dispose()
        {
            //
        }
    }
}