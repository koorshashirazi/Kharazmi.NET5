#region

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Bus
{
    /// <summary> </summary>
    public interface IBusPublisher: IMustBeInstance
    {
        /// <summary>_</summary>
        /// <param name="domainEvent"></param>
        /// <param name="publishAsynchronous"></param>
        /// <param name="token"></param>
        /// <param name="metadata"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        Task<Result> PublishAsync<TEvent>(
            [NotNull] TEvent domainEvent,
            MetadataCollection? metadata = null,
            bool? publishAsynchronous = null,
            CancellationToken token = default) where TEvent : class, IDomainEvent;
    }
}