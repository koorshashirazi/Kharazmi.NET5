#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Handlers;
using Kharazmi.Options.Domain;

#endregion

namespace Kharazmi.Pipelines
{
    [Pipeline("TransientFaultDomainEventPipeline", PipelineType.DomainEvent)]
    internal class TransientFaultEventPipeline<TEvent> : Handlers.EventHandler<TEvent>, IEventPipeline
        where TEvent : class, IDomainEvent
    {
        private readonly IEventHandler<TEvent> _handler;

        public TransientFaultEventPipeline(
            IServiceProvider sp,
            IEventHandler<TEvent> handler) : base(sp)
        {
            _handler = handler.NotNull(nameof(handler));
        }


        protected override async Task<Result> HandleAsync(CancellationToken token = default)
        {
            var result = await RetryHandler.HandlerAsync(Settings.Get<DomainOption>().RetryOption,
                () =>
                    _handler.HandleAsync(Event, DomainMetadata, token), DomainMetadata, token: token);

            return result.Value ?? Result.Fail("");
        }
    }
}