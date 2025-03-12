using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.RealTime
{
    public sealed class NullHubClientSubscriber : IHubClientSubscriber, INullInstance
    {
        public Task SubscribeAsync(Action<HubEvent> hubEventHandler)
            => Task.CompletedTask;

        public Task SubscribeAsync<T>(Action<T> messageHandler) where T : class, IMessage
            => Task.CompletedTask;

        public Task StopAsync()
            => Task.CompletedTask;
    }
}