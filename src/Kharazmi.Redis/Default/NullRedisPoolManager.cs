using System;
using Kharazmi.Dependency;
using Kharazmi.Options.Redis;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kharazmi.Redis.Default
{
    /// <summary>_</summary>
    public class NullRedisPool : IRedisPool, INullInstance
    {
        /// <summary>_</summary>
        public NullRedisPool()
        {
        }

        public IRedisDatabase RedisDatabase(RedisDbOption option) => default!;

        public IConnectionMultiplexer Connection(RedisDbOption option) => default!;

        public IServer[] GerServers(RedisDbOption option) => Array.Empty<IServer>();
    }
}