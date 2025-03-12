using System.Linq;
using Kharazmi.Caching;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.InMemory;
using Kharazmi.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    internal class CacheConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(CacheConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<CacheOption>();
            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IMemoryCache, InMemoryCache, NullMemoryCache, CacheOption>(
                (_, op) => op.UseInMemoryCache);

            services.RegisterWithFactory<IDistributedCache, HostMemoryDistributedCache, NullDistributedCache,
                CacheOption>((st, op) =>
                op.UseDistributedCache &&
                op.DistributedCacheManagerType == DistributedCacheManagerTypeConstants.HostMemory);

            services.RegisterWithFactory<IDistributedCacheExtended, HostMemoryDistributedCache, NullDistributedCache,
                CacheOption>((st, op) =>
                op.UseDistributedCache &&
                op.DistributedCacheManagerType == DistributedCacheManagerTypeConstants.HostMemory);

            services.RegisterWithFactory<ICacheManager, HostMemoryCacheManager, NullCacheManager,
                CacheOption>((_, op) =>
                op.UseInMemoryCache && op.CacheManagerType == CacheManagerTypeConstants.HostMemory);

            services.RegisterWithFactory<IDistributedCacheManager, DistributedCacheManager, NullDistributedCacheManager,
                CacheOption>((_, op) =>
                op.UseDistributedCache &&
                op.DistributedCacheManagerTypes.Contains(op.DistributedCacheManagerType) &&
                op.DistributedCacheManagerType != DistributedCacheManagerTypeConstants.ModeLess);
        }
    }
}