using System.Collections.Generic;

namespace Kharazmi.Dependency
{
    internal class ServiceFactoryEquality<T> : IEqualityComparer<T>
        where T : class
    {
        public bool Equals(T? x, T? y)
        {
            return x?.GetType() == y?.GetType();
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}