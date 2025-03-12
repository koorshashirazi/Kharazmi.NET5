using System;
using System.Net;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Connections;
using StackExchange.Redis;

namespace Kharazmi.Redis.Default
{
    internal class NullSubscriber : ISubscriber, INullInstance
    {
        public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
            => Task.FromResult(TimeSpan.Zero);

        public bool TryWait(Task task)
            => false;

        public void Wait(Task task)
        {
        }

        public T Wait<T>(Task<T> task) => default!;

        public void WaitAll(params Task[] tasks)
        {
        }

        public IConnectionMultiplexer Multiplexer => default!;

        public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
            => TimeSpan.Zero;

        public EndPoint IdentifyEndpoint(RedisChannel channel, CommandFlags flags = CommandFlags.None)
            => new UriEndPoint(new Uri(""));

        public Task<EndPoint> IdentifyEndpointAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
            => Task.FromResult<EndPoint>(new UriEndPoint(new Uri("")));

        public bool IsConnected(RedisChannel channel = new RedisChannel())
            => false;

        public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
            => 0;

        public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
            => Task.FromResult<long>(0);

        public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler,
            CommandFlags flags = CommandFlags.None)
        {
        }

        public ChannelMessageQueue Subscribe(RedisChannel channel, CommandFlags flags = CommandFlags.None)
            => default!;

        public Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler,
            CommandFlags flags = CommandFlags.None)
            => Task.CompletedTask;

        public Task<ChannelMessageQueue> SubscribeAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
            => Task.FromResult<ChannelMessageQueue>(default!);

        public EndPoint SubscribedEndpoint(RedisChannel channel)
            => new UriEndPoint(new Uri(""));

        public void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null!,
            CommandFlags flags = CommandFlags.None)
        {
        }

        public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
        {
        }

        public Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
            => Task.CompletedTask;

        public Task UnsubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null!,
            CommandFlags flags = CommandFlags.None)
            => Task.CompletedTask;
    }
}