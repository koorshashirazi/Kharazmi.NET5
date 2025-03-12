using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.RealTime
{
    public interface IHubClientPublisher : IMustBeInstance
    {
        Task PublishAsync<TMessage>(TMessage message) where TMessage : class, IMessage;
        Task PublishAsync(object message, Type messageType);
    }
}