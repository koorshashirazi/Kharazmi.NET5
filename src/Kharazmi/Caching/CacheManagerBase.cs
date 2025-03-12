using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Kharazmi.Caching
{
    public abstract class CacheManagerBase
    {
        [JsonIgnore] protected static readonly ConcurrentDictionary<string, bool> KeyHolder;
        [JsonIgnore] private CancellationTokenSource _tokenSource;
        protected readonly ISettingProvider SettingProvider;
        protected readonly ILogger<CacheManagerBase>? Logger;

        static CacheManagerBase()
        {
            KeyHolder = new ConcurrentDictionary<string, bool>();
        }

        protected CacheManagerBase(
            ISettingProvider settingProvider,
            [MaybeNull] ILoggerFactory? loggerFactory)
        {
            _tokenSource = new CancellationTokenSource();
            SettingProvider = settingProvider;
            Logger = loggerFactory.CreateLogger<CacheManagerBase>();
        }

        protected virtual Result<T> TryExecute<T>(Func<T> process)
        {
            try
            {
                return Result.OkAs(process.Invoke());
            }
            catch (Exception e)
            {
                Logger?.LogError("{Message}", e.Message);
                return Result.FailAs<T>("");
            }
        }

        protected virtual async Task<Result<T>> TryExecuteAsync<T>(Func<Task<T>> process)
        {
            try
            {
                return Result.OkAs(await process.Invoke());
            }
            catch (Exception e)
            {
                Logger?.LogError("{Message}", e.Message);
                return Result.FailAs<T>("");
            }
        }

        protected virtual async Task<Result> TryExecuteAsync(params Func<Task>[] processes)
        {
            try
            {
                var tasks = processes.Select(x => x());
                await Task.WhenAll(tasks);
                return Result.Ok();
            }
            catch (Exception e)
            {
                Logger?.LogError("{Message}", e.Message);
                return Result.Fail("");
            }
        }

        protected virtual MemoryCacheEntryOptions BuildCacheEntryOptions(
            TimeSpan? absoluteExpiresIn = default, TimeSpan? slidingExpiration = default)
        {
            var cacheOption = new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(_tokenSource.Token))
                .RegisterPostEvictionCallback(TryInvalidateCacheKey);

            var (absolute, sliding) = BuildExpirations(absoluteExpiresIn, slidingExpiration);

            cacheOption.AbsoluteExpirationRelativeToNow = absolute;
            cacheOption.SlidingExpiration = sliding;

            return cacheOption;
        }

        protected virtual DistributedCacheEntryOptions BuildDistributedCacheEntryOptions(
            TimeSpan? absoluteExpiresIn = default, TimeSpan? slidingExpiration = default)
        {
            var (absolute, sliding) = BuildExpirations(absoluteExpiresIn, slidingExpiration);
            var cacheOption = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absolute);

            if (sliding.HasValue)
                cacheOption.SetSlidingExpiration(sliding.Value);

            return cacheOption;
        }

        protected (TimeSpan absoluteExpiresIn, TimeSpan? slidingExpiration) BuildExpirations(TimeSpan? absolute,
            TimeSpan? sliding = default)
        {
            var cacheOption = SettingProvider.Get<CacheOption>();
            var defaultExpiration = cacheOption.ExpirationOption.AbsoluteExpiration;
            var defaultSliding = cacheOption.ExpirationOption.SlidingExpiration;

            var absoluteExpiresIn = defaultExpiration ?? ExpirationConstants.AbsoluteExpiration;
            var slidingExpiration = defaultSliding;

            if (absolute is not null && absolute > TimeSpan.Zero)
                absoluteExpiresIn = absolute.Value;

            if (sliding is not null)
                slidingExpiration = sliding;


            if (absoluteExpiresIn.Ticks < 0)
            {
                Logger?.LogError("Invalid absoluteExpiresIn {Absolute}", absolute);
                return (ExpirationConstants.AbsoluteExpiration, null);
            }

            if (slidingExpiration.HasValue == false)
                return (absoluteExpiresIn, slidingExpiration);

            if (slidingExpiration.Value.Ticks > 0 && slidingExpiration.Value <= absoluteExpiresIn)
                return (absoluteExpiresIn, slidingExpiration.Value);

            Logger?.LogError("Invalid sliding {Sliding}", sliding);
            return (absoluteExpiresIn, null);
        }

        protected virtual void TryInvalidateCacheKey([NotNull] object key, object value, EvictionReason reason,
            object state)
        {
            if (reason == EvictionReason.Replaced)
                return;
            TryRemoveOrInvalidateKey(key.ConvertToString());
        }

        protected virtual string TryAddKey(string key)
        {
            KeyHolder.TryAdd(key, true);
            return key;
        }

        protected virtual void TryAddKeys(IEnumerable<KeyValuePair<string, bool>> keyValues)
        {
            foreach (var (key, value) in keyValues)
                KeyHolder.TryAdd(key, value);
        }

        protected virtual string? TryRemoveOrInvalidateKey(string? key)
        {
            if (key.IsNotEmpty() && !KeyHolder.TryRemove(key, out _))
                KeyHolder.TryUpdate(key, false, true);

            return key;
        }

        protected virtual void TryRemoveInvalidateKeys()
        {
            KeyHolder.Where(k => !k.Value).ToList().AsParallel().ForAll(keyValue =>
            {
                TryRemoveOrInvalidateKey(keyValue.Key);
            });
        }

        protected virtual Task InvalidedCachesAsync()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            TryRemoveInvalidateKeys();
            return Task.CompletedTask;
        }
    }
}