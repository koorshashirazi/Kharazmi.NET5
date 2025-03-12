using Kharazmi.Dependency;
using Kharazmi.Options.Redis;
using StackExchange.Redis;

namespace Kharazmi.Redis.Default
{
    internal class NullPubSubFactory : IPubSubFactory, INullInstance
    {
        public ISubscriber GetPubSub(RedisDbOption option)
            => new NullSubscriber();
    }
}