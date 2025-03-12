using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.RealTime
{
    public interface IHubClientSubscriber : IShouldBeSingleton, IMustBeInstance
    {
        Task SubscribeAsync(Action<HubEvent> hubEventHandler);
        Task SubscribeAsync<T>(Action<T> hubEventHandler) where T : class, IMessage;
        Task StopAsync();
    }
}