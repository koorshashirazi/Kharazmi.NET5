using System;
using Kharazmi.Common.Domain;
using Kharazmi.Common.Extensions;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Events
{
    /// <summary>_</summary>
    public interface IAggregateEvent
    {
        /// <summary>_</summary>
        Type GetAggregateType();

        /// <summary>_</summary>
        string AggregateAssemblyType { get; }

        /// <summary>_</summary>
        IIdentity GetAggregateId();
    }

    /// <summary>_</summary>
    public interface IAggregateEvent<TAggregate, TKey> : IAggregateEvent
        where TAggregate : IAggregateRoot<TKey>
        where TKey : IEquatable<TKey>, IComparable
    {
        /// <summary>_</summary>
        TKey Id { get; }

        IIdentity<TKey> AggregateId();
    }

    /// <summary>_</summary>
    public abstract class AggregateEvent<TAggregate, TKey> : IAggregateEvent<TAggregate, TKey>
        where TAggregate : IAggregateRoot<TKey>
        where TKey : IEquatable<TKey>, IComparable
    {
        private readonly IIdentity<TKey> _aggregateId;

        /// <summary>_</summary>
        protected AggregateEvent(IIdentity<TKey> aggregateId)
        {
            _aggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        }


        /// <summary>_</summary>
        public Type GetAggregateType() => typeof(TAggregate);

        /// <summary>_</summary>
        public string AggregateAssemblyType => typeof(TAggregate).AssemblyQualifiedName ?? typeof(TAggregate).Name;

        /// <summary>_</summary>
        public IIdentity GetAggregateId() => _aggregateId;

        /// <summary>_</summary>
        public TKey Id => _aggregateId.ValueAs<TKey>();

        /// <summary>_</summary>
        public IIdentity<TKey> AggregateId() => _aggregateId;

      

        /// <summary>_</summary>
        public override string ToString()
            => $"{typeof(TAggregate).PrettyPrint()}/{GetType().PrettyPrint()}";
    }
}