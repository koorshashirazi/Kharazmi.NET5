#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Handlers;
using Kharazmi.Messages;
using Kharazmi.Options.Domain;

#endregion

namespace Kharazmi.Pipelines
{
    [Pipeline("TransientFaultQueryPipeline", PipelineType.DomainQuery)]
    internal class TransientFaultQueryPipeline<TQuery, TResult> : QueryHandler<TQuery, TResult>, IQueryPipeline
        where TQuery : class, IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> _handler;

        public TransientFaultQueryPipeline(
            IServiceProvider sp,
            IQueryHandler<TQuery, TResult> handler) : base(sp)

        {
            _handler = handler.NotNull(nameof(handler));
        }


        protected override async Task<Result<TResult>> HandleAsync(CancellationToken token = default)
        {
            var result = await RetryHandler.HandlerAsync(Settings.Get<DomainOption>().RetryOption,
                () =>
                    _handler.HandleAsync(Query, DomainMetadata, token), DomainMetadata, token: token);

            return result.Value ?? Result.FailAs<TResult>("");
        }
    }
}