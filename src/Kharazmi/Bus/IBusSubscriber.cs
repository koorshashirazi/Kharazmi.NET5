#region

using System;
using System.Threading;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

#endregion

namespace Kharazmi.Bus
{
    /// <summary> </summary>
    public interface IBusSubscriber: IMustBeInstance
    {
        /// <summary> </summary>
        /// <param name="onFailed"></param>
        /// <param name="token"></param>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <returns></returns>
        IBusSubscriber SubscribeTo<TDomainEvent>(
            Func<TDomainEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            MessageConfiguration? messageConfiguration = null,
            CancellationToken token = default) where TDomainEvent : class, IDomainEvent;
    }
}