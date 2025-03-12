#region

using System.Threading.Tasks;

#endregion

namespace Kharazmi.Common.Events
{
    public interface IEventStore
    {
        Task SaveAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
    }
}