using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Common.Events.Cache;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Dispatchers;
using Kharazmi.Domain;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Kharazmi.InMemory
{
    internal class HostMemoryCacheManager : CacheManagerBase, ICacheManager
    {
        private readonly IDomainMetadataFactory _domainMetadataFactory;
        private readonly IDomainDispatcher _domainDispatcher;
        private readonly IMemoryCache _cache;

        public HostMemoryCacheManager(
            ServiceFactory<IDomainDispatcher> domainDispatcher,
            ServiceFactory<IDomainMetadataFactory> domainMetadataFactory,
            ISettingProvider settingProvider,
            IMemoryCache memoryCache,
            ILoggerFactory? loggerFactory) : base(settingProvider, loggerFactory)
        {
            _domainMetadataFactory = domainMetadataFactory.Instance();
            _domainDispatcher = domainDispatcher.Instance();
            _cache = memoryCache.NotNull(nameof(memoryCache));
        }

        #region Query

        public Result<bool> Exist(string key)
            => Result.OkAs(_cache.TryGetValue(key, out _));

        public Task<Result<bool>> ExistAsync(string key, CancellationToken token = default)
            => Task.FromResult(Exist(key));

        public Result<bool> Exist<T>(T value) where T : class, ICacheItem
            => Exist(value.BuildCacheKey().CacheKey);

        public Task<Result<bool>> ExistAsync<T>(T value, CancellationToken token = default) where T : class, ICacheItem
            => ExistAsync(value.BuildCacheKey().CacheKey, token);

        public Maybe<object> Find(string key)
        {
            var isValid = _cache.TryGetValue<object>(key, out var result);
            return isValid ? result : Maybe<object>.None;
        }

        public Task<Maybe<object>> FindAsync(string key, CancellationToken token = default)
            => Task.FromResult(Find(key));

        public Maybe<T> Find<T>(string key) where T : class, ICacheItem
        {
            var isValid = _cache.TryGetValue<T>(key, out var result);
            return isValid ? result : Maybe<T>.None;
        }

        public Task<Maybe<T>> FindAsync<T>(string key, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Find<T>(key));

        public Maybe<T> Find<T>(T value) where T : class, ICacheItem
            => Find<T>(value.BuildCacheKey().CacheKey);

        public Task<Maybe<T>> FindAsync<T>(T value, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Find(value));

        public Maybe<T> FindBy<T>(Func<T, bool> predicate) where T : class, ICacheItem
        {
            var keys = KeyHolder.Keys;
            var result = Maybe<T>.None;
            
            foreach (var key in keys)
            {
                _cache.TryGetValue(key, out var cacheItem);
                if (cacheItem is not T item) continue;
                var isTrue = predicate.Invoke(item);
                if (!isTrue) continue;
                result = item;
                break;
            }

            return result;
        }

        public Task<Maybe<T>> FindByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(FindBy(predicate));

        public IEnumerable<string> GetKeys(string pattern = "*", int maxCount = 1000)
            => KeyHolder.Keys.Take(maxCount);

        public Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default)
            => Task.FromResult(GetKeys(pattern, maxCount));

        public IEnumerable<T> GetAll<T>(string pattern = "*", int maxCount = 1000) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAllAsync<T>(pattern, maxCount));

        public Task<IEnumerable<T>> GetAllAsync<T>(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem
        {
            return Task.FromResult(GetKeys(pattern, maxCount).Select(Find<T>).Where(x => x.HasValue)
                .Select(x => x.Value));
        }

        public IPagedList<T> Get<T>(int pageNumber = 0, int pageSize = 10, int maxCount = 1000)
            where T : class, ICacheItem
        {
            var paged = new PagedList<T>();

            var keys = KeyHolder.Keys.PageBy(pageNumber, pageSize);
            if (keys.Count <= 0) return paged;
            var caches = keys.Select(Find<T>).Where(x => x.HasValue).Select(x => x.Value);
            paged.AddRange(caches);
            return paged;
        }

        public Task<IPagedList<T>> GetAsync<T>(int pageNumber = 0, int pageSize = 10, int maxCount = 1000,
            CancellationToken token = default)
            where T : class, ICacheItem => Task.FromResult(Get<T>(pageNumber, pageSize, maxCount));

        public IPagedList<T> Get<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 10)
            where T : class, ICacheItem
        {
            var totalKeys = keys.ToList();

            var paged = totalKeys.PageBy(pageNumber, pageSize).Select(key => _cache.Get<T>(key)).ToList();
            return new PagedList<T>(paged, pageNumber, pageSize, totalKeys.Count);
        }

        public Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 10,
            CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Get<T>(keys, pageNumber, pageSize));

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
            Task<Result> UpdateTask(T cacheItem)
            {
                var cacheKey = cacheItem.BuildCacheKey().CacheKey;
                if (cacheKey.IsEmpty())
                {
                    Logger?.LogError("Invalid cache key with cache type {CacheType}", cacheItem.GetType().Name);
                    return Task.FromResult(
                        Result.Fail($"Invalid cache key with cache type {cacheItem.GetType().Name}"));
                }

                var result = TryExecute(() =>
                    _cache.Set(TryAddKey(cacheKey), cacheItem, BuildCacheEntryOptions(expiresIn)));

                if (result.Failed)
                {
                    Logger?.LogError("Add or update a cache item with CacheKey: {CacheKey} was failed",
                        cacheItem.CacheKey);
                    return Task.FromResult<Result>(result);
                }

                var message = new CacheAdded(cacheItem);
                return _domainDispatcher.RaiseAsync(message, message.GetType(),
                    _domainMetadataFactory.GetCurrent, token);
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
            Task<Result> UpdateTask(T cacheItem)
            {
                var cacheKey = cacheItem.BuildCacheKey().CacheKey;

                if (cacheKey.IsEmpty())
                {
                    Logger?.LogError("Invalid cache key with cache type {CacheType}", cacheItem.CacheType);
                    return Task.FromResult(Result.Fail($"Invalid cache key with cache type {cacheItem.CacheType}"));
                }

                var cache = Find<T>(cacheKey);

                if (!cache.HasValue)
                {
                    Logger?.LogError("Cache item is not found with cache type {CacheType} and cache key {CacheKey}",
                        cacheItem.CacheType, cacheKey);
                    return Task.FromResult(
                        Result.Fail(
                            $"Cache item is not found with cache type {typeof(T).Name} and cache key {cacheKey}"));
                }

                var result = TryExecute(() =>
                    _cache.Set(TryAddKey(cacheKey), value, BuildCacheEntryOptions(expiresIn)));

                if (result.Failed)
                {
                    Logger?.LogError("Execute of Operation {Operation} is failed", nameof(AddOrUpdateAsync));
                    return Task.FromResult<Result>(result);
                }

                var message = new CacheUpdated(value);
                return _domainDispatcher
                    .RaiseAsync(message, message.GetType(), _domainMetadataFactory.GetCurrent, token);
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

                keys.ForEach(key => { _cache.Remove(TryRemoveOrInvalidateKey(key)); });

                var message = new CacheRemoved(keys);

                await _domainDispatcher.RaiseAsync(message, message.GetType(),
                        _domainMetadataFactory.GetCurrent, token)
                    .ConfigureAwait(false);
            });
        }

        public Result RemoveAll()
            => AsyncHelper.RunSync(() => RemoveAllAsync());

        public Task<Result> RemoveAllAsync(CancellationToken token = default)
        {
            return TryExecuteAsync(async () =>
            {
                await InvalidedCachesAsync();
                var message = new CacheCleared();
                await _domainDispatcher
                    .RaiseAsync(message, message.GetType(), _domainMetadataFactory.GetCurrent, token)
                    .ConfigureAwait(false);
            });
        }


        public Result RemoveBy(Func<string, bool> predicate)
            => AsyncHelper.RunSync(() => RemoveByAsync(predicate));

        public async Task<Result> RemoveByAsync(Func<string, bool> predicate, CancellationToken token = default)
        {
            var keys = KeyHolder.Keys.Where(predicate).ToList();
            var tasks = keys.Select(key => RemoveAsync(new[] {new ValueKeyCacheItem(key)}, token));
            var results = await Task.WhenAll(tasks);

            return Result.Combine(results);
        }

        #endregion

        public void Dispose()
        {
            //
        }
    }
}