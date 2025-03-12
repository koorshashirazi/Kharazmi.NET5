#region

using System;
using System.Collections.Generic;
using Kharazmi.Common.Events;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Domain
{
    /// <summary></summary>
    public interface IAggregateRoot
    {
        /// <summary></summary>
        Type GetAggregateType();

        /// <summary></summary>
        [JsonProperty]
        string AssemblyType { get; }

        /// <summary> </summary>
        [JsonProperty]
        int Version { get; }

        /// <summary></summary>
        IIdentity GetAggregateId();

        /// <summary> </summary>
        [JsonProperty]
        IReadOnlyCollection<IUncommittedEvent> UncommittedEvents { get; }

        /// <summary></summary>
        void MarkAsCommitted();
    }

    /// <summary> </summary>
    public interface IAggregateRoot<TKey> : IAggregateRoot
        where TKey : IEquatable<TKey>, IComparable
    {
        /// <summary> </summary>
        TKey Id { get; }

        /// <summary> </summary>
        IIdentity<TKey> AggregateId();

        /// <summary> </summary>
        IAggregateRoot<TKey> UpdateId(IIdentity<TKey> value);
    }
}