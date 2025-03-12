#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Extensions;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Handlers
{
    public abstract class EventHandler<TEvent, TResult> : MessageHandler<TEvent>, IEventHandler<TEvent>
        where TEvent : class, IDomainEvent
        where TResult : Result
    {
        protected EventHandler(IServiceProvider sp) : base(sp)
        {
        }

        public abstract Task<Result> HandleAsync(TEvent @event, DomainMetadata domain,
            CancellationToken token = default);
        
        public Task<Result> HandleAsync(IDomainEvent domainEvent, DomainMetadata domain,
            CancellationToken token = default) => HandleAsync((TEvent) domainEvent, domain, token);
    }

    /// <summary>_</summary>
    /// <typeparam name="TEvent"></typeparam>
    public abstract class EventHandler<TEvent> : EventHandler<TEvent, Result> where TEvent : class, IDomainEvent
    {
        private TEvent? _event;

        protected TEvent Event
        {
            get => _event ?? throw new NullReferenceException(typeof(TEvent).Name);
            private set => _event = value;
        }


        /// <summary> </summary>
        /// <param name="sp"></param>
        protected EventHandler([NotNull] IServiceProvider sp) : base(sp)
        {
        }

        /// <summary></summary>
        /// <param name="event"></param>
        /// <param name="domain"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override Task<Result> HandleAsync(TEvent @event, DomainMetadata domain,
            CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Message = @event;
                _event = @event;
                MessageName = _event.GetGenericTypeName();
                Stopwatch = System.Diagnostics.Stopwatch.StartNew();
                DomainMetadata = domain;
                return HandleAsync(token);
            }
            catch (Exception e)
            {
                OnExecuteFailed();
                e.AsDomainException();
                return Task.FromResult(Result.Fail(""));
            }
        }

        protected void IncreaseDomainRetry()
        {
            base.IncreaseDomainRetry(DomainMetadata);
        }

        /// <summary></summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task<Result> HandleAsync(CancellationToken token = default);
    }
}