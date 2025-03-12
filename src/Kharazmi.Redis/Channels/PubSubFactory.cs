using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Options.Redis;
using Kharazmi.Redis.Default;
using StackExchange.Redis;

namespace Kharazmi.Redis.Channels
{
    internal class PubSubFactory : IPubSubFactory
    {
        private readonly IRedisPool _pool;

        public PubSubFactory(ServiceFactory<IRedisPool> factory)
           => _pool = factory.Instance();

        public ISubscriber GetPubSub(RedisDbOption option)
        {
            var pool = _pool.Connection(option);
            return pool.IsNull()
                ? new NullSubscriber()
                : pool.GetSubscriber();
        }
    }
}