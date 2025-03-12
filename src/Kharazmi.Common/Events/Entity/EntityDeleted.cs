using System;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Events.Entity
{
    public class EntityDeleted<TKey> : DomainEvent
        where TKey : IEquatable<TKey>, IComparable
    {
        public EntityDeleted(IIdentity<TKey> id)
        {
            SetAggregateId(id);

        }
    }
}