using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Kharazmi.Common.Domain;
using Kharazmi.Common.Events;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.AspNetCore.Core.Domain.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TrackableEntity : TrackableEntity<int>
    {
        protected TrackableEntity(IIdentity<int> id) : base(id)
        {
        }

        protected TrackableEntity(IIdentity<int> id, Action<IAggregateEvent> applier) : base(id, applier)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class TrackableEntity<TKey> : Entity<TKey>, ITrackable where TKey : IEquatable<TKey>, IComparable
    {
        protected TrackableEntity(IIdentity<TKey> id) : base(id)
        {
        }

        protected TrackableEntity(IIdentity<TKey> id, Action<IAggregateEvent> applier) : base(id, applier)
        {
        }

        /// <summary>_</summary>
        [NotMapped]
        public TrackingState TrackingState { get; set; }

        /// <summary>_</summary>
        [NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }
    }
}