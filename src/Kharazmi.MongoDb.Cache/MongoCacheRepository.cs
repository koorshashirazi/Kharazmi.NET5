using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Common.Caching;
using Kharazmi.Common.Domain;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Extensions;
using Kharazmi.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Kharazmi.MongoDb.Cache
{
    
    // TODO FullText search
    internal class MongoCacheRepository<TAggregateRootCache> : MongoRepository<TAggregateRootCache>
        where TAggregateRootCache : class, IAggregateRootCache<string>
    {
        private readonly ILogger<MongoCacheRepository<TAggregateRootCache>>? _logger;
        private readonly ICacheManager _cacheManager;

        public MongoCacheRepository(
            ICacheManager cacheManager,
            IMongoDbContext dbContext,
           ILoggerFactory loggerFactory) : base(dbContext)
        {
            _cacheManager = cacheManager;
            _logger = loggerFactory.CreateLogger<MongoCacheRepository<TAggregateRootCache>>();
        }


        public override async Task<Maybe<TAggregateRootCache>> FindByIdAsync(IIdentity<string> id)
        {
            try
            {
                var cacheKey = GetCacheKey(id);
                var cache = await _cacheManager.FindAsync(cacheKey);

                if (!cache.HasValue)
                {
                    _logger.LogWarning("Not existed cache with key {CacheKey}", cacheKey);
                    return await base.FindByIdAsync(id);
                }

                var cacheJObject = JObject.FromObject(cache.Value);

                if (!cacheJObject.HasValues)
                {
                    _logger.LogWarning("Not existed cache with key {CacheKey}", cacheKey);
                    return await base.FindByIdAsync(id);
                }

                var type = GetJArrayValue(cacheJObject, nameof(CacheItem.CacheType));

                if (type.IsEmpty())
                {
                    _logger.LogWarning("Invalid cache type with type {CacheType} and key {CacheKey}",
                        nameof(CacheItem.CacheType), cacheKey);
                    return await base.FindByIdAsync(id);
                }

                var typeValue = Type.GetType(type);
                var instance = typeof(TAggregateRootCache).CreateInstance<TAggregateRootCache>();

                if (typeValue is null || instance is null)
                {
                    _logger.LogWarning("Invalid cache type with type {CacheType} and key {CacheKey}",
                        nameof(CacheItem.CacheType), cacheKey);
                    return await base.FindByIdAsync(id);
                }

                if (instance.GetFromCache(cacheJObject.ToObject(typeValue)) is TAggregateRootCache value)
                {
                    _logger?.LogTrace(
                        "Get aggregate cache value with aggregate type {AggregateEventType} and cache type {CacheType} and cache key {CacheKey}",
                        value.GetType().Name, nameof(CacheItem.CacheType), cacheKey);
                    return value;
                }

                _logger.LogWarning("Invalid aggregate cache type with type {CacheType} and key {CacheKey}",
                    nameof(CacheItem.CacheType), cacheKey);

                return await base.FindByIdAsync(id);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Maybe<TAggregateRootCache>.None;
            }
        }

        public override async Task AddAsync(TAggregateRootCache entity, bool useTransaction = false)
        {
            await DbContext.AddCommandAsync($"InsertCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                token => _cacheManager.AddOrUpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);
            await base.AddAsync(entity).ConfigureAwait(false);
        }

        protected override async Task AddTransactionAsync(TAggregateRootCache entity)
        {
            await DbContext.AddTransactionCommandAsync($"InsertCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                (_, token) => _cacheManager.AddOrUpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);
            await base.AddTransactionAsync(entity).ConfigureAwait(false);
        }

        public override async Task AddAsync(IEnumerable<TAggregateRootCache> entities, bool useTransaction = false)
        {
            var aggregates = entities.ToArray();

            foreach (var entity in aggregates)
                await DbContext.AddCommandAsync($"InsertCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                    token => _cacheManager.AddOrUpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.AddAsync(aggregates).ConfigureAwait(false);
        }

        protected override async Task AddTransactionAsync(IEnumerable<TAggregateRootCache> entities)
        {
            var aggregates = entities.ToArray();

            foreach (var entity in aggregates)
                await DbContext.AddTransactionCommandAsync($"InsertCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                    (_, token) => _cacheManager.AddOrUpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.AddTransactionAsync(aggregates).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(TAggregateRootCache entity, bool useTransaction = false)
        {
            await DbContext.AddCommandAsync($"UpdateCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                token => _cacheManager.UpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.UpdateAsync(entity).ConfigureAwait(false);
        }

        protected override async Task UpdateTransactionAsync(TAggregateRootCache entity)
        {
            await DbContext.AddTransactionCommandAsync($"UpdateCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                (_, token) => _cacheManager.UpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.UpdateTransactionAsync(entity).ConfigureAwait(false);
        }

        public override async Task UpdateAsync(IEnumerable<TAggregateRootCache> entities, bool useTransaction = false)
        {
            var aggregates = entities.ToArray();

            foreach (var entity in aggregates)
                await DbContext.AddCommandAsync($"UpdateCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                    token => _cacheManager.UpdateAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.UpdateAsync(aggregates).ConfigureAwait(false);
        }

        protected override async Task UpdateTransactionAsync(IEnumerable<TAggregateRootCache> entities)
        {
            var aggregates = entities.ToArray();

            foreach (var entity in aggregates)
                await DbContext.AddTransactionCommandAsync($"UpdateCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                        (_, token) => _cacheManager.UpdateAsync(new [] {entity}, token: token))
                    .ConfigureAwait(false);

            await base.UpdateTransactionAsync(aggregates).ConfigureAwait(false);
        }

        public override async Task DeleteAsync(TAggregateRootCache entity, bool useTransaction = false)
        {
            await DbContext.AddCommandAsync($"RemoveCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                token => _cacheManager.RemoveAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.DeleteAsync(entity).ConfigureAwait(false);
        }

        protected override async Task DeleteTransactionAsync(TAggregateRootCache entity)
        {
            await DbContext.AddTransactionCommandAsync($"RemoveCache-{entity.GetType().Name}-{Guid.NewGuid():N}",
                (_, token) => _cacheManager.RemoveAsync(new [] {entity}, token: token)).ConfigureAwait(false);

            await base.DeleteTransactionAsync(entity).ConfigureAwait(false);
        }

        public override async Task DeleteAsync(IEnumerable<TAggregateRootCache> entities, bool useTransaction = false)
        {
            var aggregates = entities.ToArray();

            await DbContext.AddCommandAsync($"RemoveCache{Guid.NewGuid():N}",
                token => _cacheManager.RemoveAsync(aggregates, token)).ConfigureAwait(false);
            await base.DeleteAsync(aggregates).ConfigureAwait(false);
        }

        protected override async Task DeleteTransactionAsync(IEnumerable<TAggregateRootCache> entities)
        {
            var aggregates = entities.ToArray();
            await DbContext.AddTransactionCommandAsync($"RemoveCache:{Guid.NewGuid():N}",
                (_, token) => _cacheManager.RemoveAsync(aggregates, token)).ConfigureAwait(false);

            await base.DeleteTransactionAsync(aggregates).ConfigureAwait(false);
        }

        private string? GetJArrayValue(JObject jArray, string key)
        {
            foreach (var (k, value) in jArray)
            {
                if (key == k)
                {
                    return value != null && value.HasValues ? value.Value<string>() : default;
                }
            }

            return default;
        }

        private static string GetCacheKey(IIdentity<string> id)
        {
            var aggregate = typeof(TAggregateRootCache).CreateInstance<TAggregateRootCache>(id);
            aggregate?.UpdateId(id);
            var cacheKey = aggregate?.BuildCacheKey().CacheKey;
            return cacheKey ?? id.Value;
        }
    }
}