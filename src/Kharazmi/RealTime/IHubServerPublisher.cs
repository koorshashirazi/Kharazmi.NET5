using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.RealTime
{
    public interface IHubServerPublisher : IMustBeInstance
    {
        Task PublishAsync(object message, Type messageType);
        Task PublishAsync<TMessage>(TMessage message) where TMessage : class, IMessage;
    }
}