using System;
using System.Threading;
using Kharazmi.BuilderExtensions;
using Kharazmi.Caching;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Localization;
using Kharazmi.Localization.Default;
using Kharazmi.Options.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.MongoDb.Cache
{
    internal class MongoCacheFactory : MongoFactory
    {
        private readonly IServiceProvider _sp;
        private readonly ISettingProvider _settings;
        private readonly object _lock = new();

        public MongoCacheFactory(
            IServiceProvider sp,
            ServiceFactory<IMongoDbContext> mongoDbFactory) : base(mongoDbFactory)
        {
            _sp = sp;
            _settings = mongoDbFactory.Settings;
        }

        public override IMongoRepository<TAggregateRootCache> Cache<TAggregateRootCache>()
        {
            try
            {
                Monitor.Enter(_lock);

                var logger = LoggerFactory.CreateLogger<MongoCacheRepository<TAggregateRootCache>>() ??
                             NullLogger<MongoCacheRepository<TAggregateRootCache>>.Instance;

                var mongoOptions = _settings.Get<MongoOptions>();
                var cacheOption = mongoOptions.SecondLevelCacheOption;
                if (cacheOption is null || mongoOptions.UseSecondLevelCache == false)
                {
                    logger.LogError(MessageTemplate.MongoCacheDisabled, MessageEventName.MongoCache,
                        nameof(MongoCacheFactory));
                    return new NullMongoRepository<TAggregateRootCache>();
                }

                var providerOptionKey = cacheOption.CacheProviderOptionKey;
                if (providerOptionKey.IsEmpty())
                {
                    logger.LogError(MessageTemplate.NotFoundOption, MessageEventName.MongoCache,
                        nameof(MongoCacheFactory), nameof(SecondLevelCacheOption), providerOptionKey);
                    return new NullMongoRepository<TAggregateRootCache>();
                }

                var cacheManager = _sp.GetInstance<ICacheManager>();
                switch (cacheOption.CacheProvider)
                {
                    case CacheManagerTypeConstants.ModeLess:
                        return new NullMongoRepository<TAggregateRootCache>();
                    case CacheManagerTypeConstants.HostMemory:
                    case CacheManagerTypeConstants.RedisDb:
                        return new MongoCacheRepository<TAggregateRootCache>(cacheManager, DbContext, LoggerFactory);
                    case CacheManagerTypeConstants.Distributed:
                        var distributedCache = _sp.GetInstance<IDistributedCacheManager>();
                        return new MongoCacheRepository<TAggregateRootCache>(distributedCache, DbContext,
                            LoggerFactory);
                    default:
                        return new NullMongoRepository<TAggregateRootCache>();
                }
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}