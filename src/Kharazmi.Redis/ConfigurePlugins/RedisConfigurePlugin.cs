using Kharazmi.Caching;
using Kharazmi.Channels;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options;
using Kharazmi.Options.Redis;
using Kharazmi.Redis.Channels;
using Kharazmi.Redis.Default;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;

namespace Kharazmi.Redis.ConfigurePlugins
{
    internal class RedisConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(RedisConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<RedisDbOptions>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.ReplaceService<ISerializer, RedisJsonSerializer>(ServiceLifetime.Singleton);
            services.ReplaceService<IChannelJsonSerializer, RedisJsonSerializer>(ServiceLifetime.Singleton);
            services.ReplaceService<ICacheItemSerializer, RedisJsonSerializer>(ServiceLifetime.Singleton);

            services.RegisterWithFactory<IRedisPool, RedisPool, NullRedisPool, RedisDbOptions>((_, op) => op.Enable);

            services.RegisterWithFactory<IRedisCacheManager, RedisCacheManager, NullRedisCacheManager,
                RedisDbOptions>((st, op) =>
                op.Enable && st.Get<CacheOption>().CacheManagerType == CacheManagerTypeConstants.RedisDb);

            services.RegisterWithFactory<ICacheManager, RedisCacheManager, NullRedisCacheManager,
                RedisDbOptions>((st, op) =>
                op.Enable && st.Get<CacheOption>().CacheManagerType == CacheManagerTypeConstants.RedisDb);

            services.RegisterWithFactory<IChannelPublisher, RedisBusPublisher, NullRedisChannelPublisher,
                RedisDbOptions>((_, op) => op.UseChannelPublisher);
        }
    }
}