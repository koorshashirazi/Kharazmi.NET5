using System.Threading.Tasks;
using Kharazmi.Common.Events;

namespace Kharazmi.RealTime
{
    public interface IUserHubNotifier
    {
        Task OnPublish<TMessage>(TMessage message) where TMessage : class, IMessage;

        Task OnSubscribe<TMessage>(string connectionId) where TMessage : class, IMessage;

        Task OnUnSubscribe<TMessage>(string connectionId, string topic) where TMessage : class, IMessage;
    }
}