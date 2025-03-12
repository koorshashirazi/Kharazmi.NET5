using System;

namespace Kharazmi.Common.ValueObjects
{
    /// <summary> </summary>
    public interface IIdentity : ISingleValueObject
    {
    }

    /// <summary>_</summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IIdentity<out TKey> : ISingleValueObject<TKey>, IIdentity
        where TKey : IEquatable<TKey>, IComparable
    {
    }

    /// <summary>_</summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class Identity<T, TKey> : SingleValueObject<TKey>, IIdentity<TKey>
        where TKey : IEquatable<TKey>, IComparable
        where T : IIdentity<TKey>

    {
        /// <summary>_</summary>
        /// <param name="value"></param>
        protected Identity(TKey value) : base(value)
        {
        }
    }
}