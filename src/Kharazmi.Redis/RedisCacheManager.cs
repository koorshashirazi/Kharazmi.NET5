using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Common.Events.Cache;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Json;
using Kharazmi.Options.Redis;
using Kharazmi.Redis.Extensions;
using Kharazmi.Threading;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kharazmi.Redis
{
    internal class RedisCacheManager : CacheManagerBase, IRedisCacheManager
    {
        private readonly ISerializer _serializer;
        private readonly IRedisPool _redisPool;
        private readonly ICacheItemSerializer _cacheSerializer;

        public RedisCacheManager(
            ISettingProvider settingProvider,
            ServiceFactory<IRedisPool> factory,
            ICacheItemSerializer cacheSerializer,
            ISerializer serializer,
            [MaybeNull] ILoggerFactory? loggerFactory) : base(settingProvider, loggerFactory)
        {
            _cacheSerializer = cacheSerializer;
            _serializer = serializer;
            RedisOption = SettingProvider.GetRedisOption("");
            _redisPool = factory.Instance();
        }

        private IRedisDatabase Db => _redisPool.RedisDatabase(RedisOption);

        public IRedisCacheManager SetDatabaseTo(RedisDbOption option)
        {
            RedisOption = option;
            return this;
        }

        #region Query

        public Result<bool> Exist(string key)
            => AsyncHelper.RunSync(() => ExistAsync(key));

        public async Task<Result<bool>> ExistAsync(string key, CancellationToken token = default)
        {
            var result = await TryExecuteAsync(() => Db.ExistsAsync(key));
            if (!result.Failed) return result;
            Logger?.LogError("Execute of Operation {Operation} is failed", nameof(ExistAsync));
            return result;
        }

        public Result<bool> Exist<T>(T value) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => ExistAsync(value.BuildCacheKey().CacheKey));

        public Task<Result<bool>> ExistAsync<T>(T value, CancellationToken token = default) where T : class, ICacheItem
            => ExistAsync(value.BuildCacheKey().CacheKey, token);

        public Maybe<object> Find(string key)
            => AsyncHelper.RunSync(() => FindAsync(key));

        public async Task<Maybe<object>> FindAsync(string key, CancellationToken token = default)
        {
            var result = await TryExecuteAsync<Maybe<object>>(async () => await Db.GetAsync<object>(key));
            if (!result.Failed) return result.Value;
            Logger?.LogError("Execute of Operation {Operation} is failed", nameof(FindAsync));
            return Maybe<object>.None;
        }

        public Maybe<T> Find<T>(string key) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => FindAsync<T>(key));

        public async Task<Maybe<T>> FindAsync<T>(string key, CancellationToken token = default)
            where T : class, ICacheItem
        {
            var result = await TryExecuteAsync<Maybe<T>>(async () => await Db.GetAsync<T>(key));
            if (!result.Failed) return result.Value;
            Logger?.LogError("Execute of Operation {Operation} is failed", nameof(FindAsync));
            return Maybe<T>.None;
        }

        public Maybe<T> Find<T>(T value) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => FindAsync(value));

        public Task<Maybe<T>> FindAsync<T>(T value, CancellationToken token = default)
            where T : class, ICacheItem => FindAsync<T>(value.BuildCacheKey().CacheKey, token);

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

        public IEnumerable<string> GetKeys(string pattern = "*", int total = 1000)
            => AsyncHelper.RunSync(() => GetKeysAsync(pattern, total));

        public async Task<IEnumerable<string>> GetKeysAsync(string pattern, int total = 1000,
            CancellationToken token = default)
        {
            var keyPrefix = RedisOption.KeyPrefix;

            pattern = $"{keyPrefix}{pattern}";

            var keys = new HashSet<string>();
            var servers = _redisPool.GerServers(RedisOption);

            if (servers.Length == 0)
                throw new DistributedCacheException("No server found to serve the KEYS command");

            foreach (var unused in servers)
            {
                long nextCursor = 0;
                do
                {
                    var redisResult = await Db.Database
                        .ExecuteAsync("SCAN", nextCursor.ToString(), "MATCH", pattern, "COUNT", $"{total}")
                        .ConfigureAwait(false);
                    var innerResult = (RedisResult[]) redisResult;

                    nextCursor = long.Parse((string) innerResult[0]);

                    var resultLines = ((string[]) innerResult[1]).ToArray();
                    keys.UnionWith(resultLines);
                } while (nextCursor != 0);
            }

            return keyPrefix.IsNotEmpty()
                ? keys.Select(k => k.Substring(keyPrefix.Length))
                : keys;
        }

        public IEnumerable<T> GetAll<T>(string pattern = "*", int maxCount = 1000) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAllAsync<T>(pattern, maxCount));

        public async Task<IEnumerable<T>> GetAllAsync<T>(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem
        {
            var keys = await GetKeysAsync(pattern, maxCount, token);
            var keyValues = await Db.GetAllAsync<T>(keys);
            return keyValues.Where(x => x.Value != null).Select(x => x.Value);
        }

        public IPagedList<T> Get<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000)
            where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAsync<T>(pageNumber, pageSize, maxCount));

        public async Task<IPagedList<T>> GetAsync<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem
        {
            var result = await TryExecuteAsync(async () =>
            {
                var searchKeys = await GetKeysAsync("*", maxCount, token);
                return await GetAsync<T>(searchKeys, pageNumber, pageSize, token);
            });

            if (!result.Failed) return result.Value ?? PagedList<T>.Empty;
            Logger?.LogError(MessageTemplate.GetCacheItemResult, MessageEventName.GetCacheItem,
                nameof(RedisCacheManager), result.Description);
            return PagedList<T>.Empty;
        }

        public IPagedList<T> Get<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100)
            where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetAsync<T>(keys, pageNumber, pageSize));

        public async Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100,
            CancellationToken token = default) where T : class, ICacheItem
        {
            var result = await TryExecuteAsync(async () =>
            {
                var totalKeys = keys.ToList();
                var pagedKeys = totalKeys.PageBy(pageNumber, pageSize);
                var keyValues = await Db.GetAllAsync<T>(pagedKeys);
                var source = keyValues.Where(x => x.Value != null).Select(x => x.Value);

                return new PagedList<T>(source, pageNumber, pageSize, totalKeys.Count);
            });

            if (!result.Failed) return result.Value ?? PagedList<T>.Empty;
            Logger?.LogError(MessageTemplate.GetCacheItemResult, MessageEventName.GetCacheItem,
                nameof(RedisCacheManager), result.Description);
            return PagedList<T>.Empty;
        }

        public IEnumerable<T> GetBy<T>(Func<T, bool> predicate) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => GetByAsync(predicate));

        public async Task<IEnumerable<T>> GetByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem
        {
            var items = await GetAsync<T>(0, 1000, 1000, token);
            return items.Where(predicate);
        }


        #region IRedisCacheManager

        public RedisDbOption RedisOption { get; private set; }


        public IPagedList<ICacheItem> GetCacheItems(IEnumerable<string> keys, int page = 0, int pageSize = 10)
            => AsyncHelper.RunSync(() => GetCacheItemsAsync(keys, page, pageSize));

        public async Task<IPagedList<ICacheItem>> GetCacheItemsAsync(IEnumerable<string> keys, int page = 0,
            int pageSize = 10, CancellationToken token = default)
        {
            var result = await TryExecuteAsync<IPagedList<ICacheItem>>(async () =>
            {
                var redisKeys = keys.PageBy(page, pageSize).Select(x => (RedisKey) x).ToArray();
                var redisValues = await Db.Database.StringGetAsync(redisKeys).ConfigureAwait(false);
                var cacheItems = new PagedList<ICacheItem> {TotalPages = redisKeys.Length};

                foreach (var redisValue in redisValues)
                {
                    if (!redisValue.HasValue) continue;
                    var option = _cacheSerializer.DeserializeCacheItem(redisValue);
                    if (option is Some<ICacheItem> some)
                        cacheItems.Add(some.Value);
                }

                return cacheItems;
            });
            if (!result.Failed) return result.Value ?? new PagedList<ICacheItem>();
            Logger?.LogError("Execute of Operation {Operation} is failed", nameof(GetCacheItemsAsync));
            return PagedList<ICacheItem>.Empty;
        }

        public IPagedList<ICacheItem> GetCacheItems(string pattern = "*", int page = 0, int pageSize = 10)
            => AsyncHelper.RunSync(() => GetCacheItemsAsync(pattern, page, pageSize));

        public async Task<IPagedList<ICacheItem>> GetCacheItemsAsync(string pattern = "*", int page = 0,
            int pageSize = 10, CancellationToken token = default)
        {
            var total = page == 0 ? pageSize : page * pageSize;
            var searchKeys = await GetKeysAsync(pattern, total, token);
            var keys = searchKeys.ToList();

            if (keys.Count <= 0) return new PagedList<ICacheItem>();
            return await GetCacheItemsAsync(keys, page, pageSize, token);
        }

        #endregion

        #endregion

        #region Command

        public Result AddOrUpdate<T>(T[] value, TimeSpan? expiresIn = default) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => AddOrUpdateAsync(value, expiresIn));

        public Task<Result> AddOrUpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
        {
            Task[] UpdateTask(ITransaction transaction, IEnumerable<T> cacheItems)
            {
                var tasks = new List<Task>();

                foreach (var cacheItem in cacheItems)
                {
                    var cacheKey = cacheItem.BuildCacheKey().CacheKey;
                    var (absolute, _) = BuildExpirations(expiresIn);
                    var entryBytes = cacheItem.OfValueSize(_serializer, RedisOption.MaxValueLength, cacheKey);
                    var channelEvent = new CacheAdded(cacheItem).ChannelName(RedisOption.ChannelPrefix);
                    var message = channelEvent.Serialize();

                    var t1 = transaction.StringSetAsync(cacheKey, entryBytes, absolute);
                    var t2 = transaction.PublishAsync(channelEvent.DomainMessageMetadata.ChannelName, message,
                        CommandFlags.FireAndForget);

                    Logger?.LogTrace("Add or update cache with type {CacheType} and cache key {CacheKey}",
                        cacheItem.CacheType, cacheKey);
                    tasks.Add(t1);
                    tasks.Add(t2);
                }

                return tasks.ToArray();
            }

            return TryExecuteAsync(() => { return TransactionAsync(transaction => UpdateTask(transaction, value)); });
        }


        public Result Update<T>(T[] value, TimeSpan? expiresIn = default)
            where T : class, ICacheItem
            => AsyncHelper.RunSync(() => UpdateAsync(value, expiresIn));

        public Task<Result> UpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
        {
            Task[] UpdateTask(ITransaction transaction, IEnumerable<T> cacheItems)
            {
                var tasks = new List<Task>();

                foreach (var cacheItem in cacheItems)
                {
                    var cacheKey = cacheItem.BuildCacheKey().CacheKey;
                    var (absolute, _) = BuildExpirations(expiresIn);
                    var entryBytes = cacheItem.OfValueSize(_serializer, RedisOption.MaxValueLength, cacheKey);
                    var channelEvent = new CacheAdded(cacheItem).ChannelName(RedisOption.ChannelPrefix);
                    var message = channelEvent.Serialize();

                    var t1 = transaction.StringSetAsync(cacheKey, entryBytes, absolute, When.Exists);
                    var t2 = transaction.PublishAsync(channelEvent.DomainMessageMetadata.ChannelName, message,
                        CommandFlags.FireAndForget);

                    Logger?.LogTrace("Add or update cache with type {CacheType} and cache key {CacheKey}",
                        cacheItem.CacheType, cacheKey);
                    tasks.Add(t1);
                    tasks.Add(t2);
                }

                return tasks.ToArray();
            }

            return TryExecuteAsync(() => { return TransactionAsync(transaction => UpdateTask(transaction, value)); });
        }

        public Result Remove<T>(T[] value) where T : class, ICacheItem
            => AsyncHelper.RunSync(() => RemoveAsync(value));

        public Task<Result> RemoveAsync<T>(T[] value, CancellationToken token = default)
            where T : class, ICacheItem
        {
            return TryExecuteAsync(() =>
            {
                value.NotNull(nameof(value));

                var keys = value.Select(x => x.BuildCacheKey().CacheKey).ToList();
                var redisKeys = keys.Select(x => (RedisKey) x).ToArray();
                var channelEvent = new CachesRemoved(keys).ChannelName(RedisOption.ChannelPrefix);
                var message = channelEvent.Serialize();

                return TransactionAsync(transaction =>
                {
                    var t1 = transaction.KeyDeleteAsync(redisKeys);
                    var t2 = transaction.PublishAsync(channelEvent.DomainMessageMetadata.ChannelName, message,
                        CommandFlags.FireAndForget);
                    return new Task[] {t1, t2};
                });
            });
        }

        public Result RemoveAll()
            => AsyncHelper.RunSync(() => RemoveAllAsync());

        public Task<Result> RemoveAllAsync(CancellationToken token = default)
        {
            return TryExecuteAsync(async () =>
            {
                var channelEvent = new CacheCleared().ChannelName(RedisOption.ChannelPrefix);
                var message = channelEvent.Serialize();

                await TransactionAsync(transaction =>
                {
                    var endPoints = transaction.Multiplexer.GetEndPoints();
                    var tasks = new List<Task>(endPoints.Length);

                    foreach (var endpoint in endPoints)
                    {
                        var server = transaction.Multiplexer.GetServer(endpoint);
                        if (!server.IsReplica)
                            tasks.Add(server.FlushDatabaseAsync(Db.Database.Database));
                    }

                    var t2 = transaction.PublishAsync(channelEvent.DomainMessageMetadata.ChannelName, message,
                        CommandFlags.FireAndForget);

                    tasks.Add(t2);

                    return tasks.ToArray();
                }).ConfigureAwait(false);
            });
        }

        public Result RemoveBy(Func<string, bool> predicate)
            => AsyncHelper.RunSync(() => RemoveByAsync(predicate));

        public Task<Result> RemoveByAsync(Func<string, bool> predicate, CancellationToken token = default)
        {
            return TryExecuteAsync(async () =>
            {
                var totalKeys = await GetKeysAsync("*", 1000, token);
                var keys = totalKeys.Where(predicate).ToArray();
                var channelEvent = new CachesRemoved(keys).ChannelName(RedisOption.ChannelPrefix);
                var message = channelEvent.Serialize();

                await TransactionAsync(transaction =>
                {
                    var redisKeys = keys.Select(x => (RedisKey) x).ToArray();
                    var t1 = transaction.KeyDeleteAsync(redisKeys);
                    var t2 = transaction.PublishAsync(channelEvent.DomainMessageMetadata.ChannelName, message,
                        CommandFlags.FireAndForget);
                    return new Task[] {t1, t2};
                });
            });
        }

        public void Dispose()
        {
            //
        }

        private async Task TransactionAsync(Func<ITransaction, Task[]> execute)
        {
            var database = Db.Database;
            var shouldCommit = false;

            ITransaction transaction;
            if (database is ITransaction tran)
            {
                transaction = tran;
            }
            else if (database is { } db)
            {
                transaction = db.CreateTransaction();
                shouldCommit = true;
            }
            else
            {
                throw new NotSupportedException("The database instance type is not supported");
            }

            var tasks = execute.Invoke(transaction);

            if (shouldCommit && !await transaction.ExecuteAsync().ConfigureAwait(false))
                throw new Exception("The transaction has failed");

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion
    }
}