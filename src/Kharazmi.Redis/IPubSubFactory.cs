using Kharazmi.Dependency;
using Kharazmi.Options.Redis;
using StackExchange.Redis;

namespace Kharazmi.Redis
{
    /// <summary>_</summary>
    public interface IPubSubFactory : IShouldBeSingleton, IMustBeInstance
    {
        ISubscriber GetPubSub(RedisDbOption option);
    }
}