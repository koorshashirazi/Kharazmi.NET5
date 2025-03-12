#region

using System;
using Kharazmi.Common.ValueObjects;

#endregion

namespace Kharazmi.Common.Domain
{
    /// <summary>
    ///     Interface base entity
    /// </summary>
    public interface IEntity<out TKey> : IEntity
        where TKey : IEquatable<TKey>, IComparable
    {
        /// <summary></summary>
        TKey Id { get; }

        /// <summary> </summary>
        /// <returns></returns>
        IIdentity<TKey> EntityId();
    }

    /// <summary> </summary>
    public interface IEntity 
    {
        /// <summary> </summary>
        /// <returns></returns>
        IIdentity GetEntityId();
    }
}