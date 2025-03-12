using System.Collections.Generic;

namespace Kharazmi.Common.Events
{
    internal class UncommittedEventEquality: IEqualityComparer<IUncommittedEvent>
    {
        public bool Equals(IUncommittedEvent? x, IUncommittedEvent? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.AggregateEvent.Equals(y.AggregateEvent);
        }

        public int GetHashCode(IUncommittedEvent obj)
        {
            return obj.AggregateEvent.GetHashCode();
        }
    }
}