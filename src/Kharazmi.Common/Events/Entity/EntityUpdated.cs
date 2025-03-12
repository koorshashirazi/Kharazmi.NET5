using System;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Events.Entity
{
    public class EntityUpdated<TKey> : DomainEvent
        where TKey : IEquatable<TKey>, IComparable
    {
        public EntityUpdated(IIdentity<TKey> id)
        {
            SetAggregateId(id);

        }
    }
}