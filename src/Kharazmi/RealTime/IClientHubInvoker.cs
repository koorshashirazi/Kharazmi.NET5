using System.Threading.Tasks;
using Kharazmi.Common.Events;

namespace Kharazmi.RealTime
{
    public interface IClientHubInvoker
    {
        Task OnPublish(HubEvent hubEvent);
    }
}