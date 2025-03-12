using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Messages;

namespace Kharazmi.Handlers
{
    public abstract class QueryHandler<TQuery, TResult> : MessageHandler<TQuery>, IQueryHandler<TQuery, TResult>
        where TQuery : class, IQuery<TResult>
    {
        private TQuery? _query;

        public TQuery Query
        {
            get => _query ?? throw new NullReferenceException(typeof(TQuery).Name);
            set => _query = value;
        }

        protected QueryHandler(IServiceProvider sp) : base(sp)
        {
        }

        public Task<Result<TResult>> HandleAsync(TQuery query, DomainMetadata domain,
            CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Message = query;
                _query = query;
                MessageName = query.GetGenericTypeName();
                Stopwatch = System.Diagnostics.Stopwatch.StartNew();
                DomainMetadata = domain;
                return HandleAsync(token);
            }
            catch (Exception e)
            {
                OnExecuteFailed();
                e.AsDomainException();
                return Task.FromResult(Result.FailAs<TResult>(""));
            }
        }

        public Task<Result<TResult>> HandleAsync(IQuery<TResult> query, DomainMetadata domain,
            CancellationToken token = default)
            => HandleAsync((TQuery) query, domain, token);

        protected abstract Task<Result<TResult>> HandleAsync(CancellationToken token = default);
    }
}