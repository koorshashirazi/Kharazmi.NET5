using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.RealTime
{
    public interface IHubServerSubscriber : IShouldBeSingleton, IMustBeInstance
    {
        Task SubscribeAsync<TMessage>() where TMessage : class, IMessage;
        Task SubscribeAsync(Type messageType);
    }
}