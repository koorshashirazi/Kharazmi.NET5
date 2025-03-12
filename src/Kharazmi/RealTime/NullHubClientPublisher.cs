using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.RealTime
{
    public sealed class NullHubClientPublisher : IHubClientPublisher, INullInstance
    {
        public Task PublishAsync<TMessage>(TMessage message) where TMessage : class, IMessage
            => Task.CompletedTask;

        public Task PublishAsync(object message, Type messageType)
            => Task.CompletedTask;
    }
}