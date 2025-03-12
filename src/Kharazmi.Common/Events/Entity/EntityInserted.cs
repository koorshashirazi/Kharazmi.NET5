using System;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Events.Entity
{
    public class EntityInserted<TKey> : DomainEvent
        where TKey : IEquatable<TKey>, IComparable
    {
        public EntityInserted(IIdentity<TKey> id)
        {
            SetAggregateId(id);
        }
    }
}