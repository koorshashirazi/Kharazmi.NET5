#region

using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Common.Events;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Domain
{
    /// <summary>
    /// Base entity with base id value
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class Entity<TKey> : IEntity<TKey>, IInternalEventHandler
        where TKey : IEquatable<TKey>, IComparable
    {
        private readonly IIdentity<TKey> _id;
        private readonly Action<IAggregateEvent>? _applier;

        /// <summary> </summary>
        private Entity()
        {
        }
        
        /// <summary>_</summary>
        /// <param name="id"></param>
        protected Entity(IIdentity<TKey> id)
        {
            _id = id;
        }

        /// <summary> </summary>
        /// <param name="id"></param>
        /// <param name="applier"></param>
        protected Entity(IIdentity<TKey> id, Action<IAggregateEvent> applier)
        {
            _id = id;
            _applier = applier;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public IIdentity GetEntityId() =>
            _id ?? throw new NullReferenceException($"The {GetType().Name} is not instanced using a id");

        /// <summary>Gets or sets the primary key for this role.</summary>
        [JsonProperty]
        public TKey Id => _id.Value;

        /// <returns></returns>
        public IIdentity<TKey> EntityId() => _id;

        /// <summary> </summary>
        [JsonIgnore]
        protected virtual object This => this;


        /// <summary>_</summary>
        /// <param name="event"></param>
        protected virtual void When(IAggregateEvent @event)
        {
        }

        /// <summary>_</summary>
        /// <param name="event"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void Emit(IAggregateEvent @event)
        {
            When(@event);
            if (_applier is null)
                throw new InvalidOperationException(
                    $"Entity '{GetType().Name}' does not have an 'applier' that takes aggregate event '{@event.GetType().Name}' as argument. Ensure adjusted a applier");

            _applier.Invoke(@event);
        }

        void IInternalEventHandler.Handle(IAggregateEvent @event) => When(@event);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool IsTransient()
        {
            if (EqualityComparer<TKey>.Default.Equals(Id, default)) return true;

            if (typeof(TKey) == typeof(int)) return Convert.ToInt32(Id) <= 0;
            if (typeof(TKey) == typeof(long)) return Convert.ToInt64(Id) <= 0;

            return false;
        }


        #region Value object

        /// <summary> </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            var value = (Entity<TKey>) obj;
            return EqualityValues().SequenceEqual(value.EqualityValues());
        }

        /// <summary> </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return EqualityValues().Aggregate(17, (current, next) =>
            {
                unchecked
                {
                    return current * 23 + (next?.GetHashCode() ?? 0);
                }
            });
        }

        /// <summary></summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        /// <summary></summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
        {
            return !(left == right);
        }

        /// <summary></summary>
        protected virtual IEnumerable<object> EqualityValues()
        {
            yield return Id;
        }

        #endregion
    }
}