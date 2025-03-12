namespace Kharazmi.Common.Events
{
    /// <summary>_</summary>
    public interface IInternalEventHandler
    {
        /// <summary>_</summary>
        /// <param name="event"></param>
        void Handle(IAggregateEvent @event);
    }
}