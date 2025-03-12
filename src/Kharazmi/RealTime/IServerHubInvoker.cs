using System;
using System.Threading.Tasks;
using Kharazmi.Common.Events;

namespace Kharazmi.RealTime
{
    public interface IServerHubInvoker
    {
        Task OnPublish(object message, Type messageType);
        Task OnPublish<TMessage>() where TMessage : class, IMessage;
        Task OnSubscribe<TMessage>() where TMessage : class, IMessage;
        Task OnSubscribe(Type messageType);
        Task OnUnSubscribe<TMessage>() where TMessage : class, IMessage;
        Task OnUnSubscribe(Type messageType);
    }
}