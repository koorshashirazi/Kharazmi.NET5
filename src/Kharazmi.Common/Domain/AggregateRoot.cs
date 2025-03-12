#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Domain
{
    /// <summary></summary>
    public abstract class AggregateRoot<TKey> : IAggregateRoot<TKey>,
        IInternalEventHandler, IEquatable<AggregateRoot<TKey>>
        where TKey : IEquatable<TKey>, IComparable
    {
        // private readonly ConcurrentDictionary<Type, Action<IAggregateEvent>> _appliers =
        //     new ConcurrentDictionary<Type, Action<IAggregateEvent>>();

        private readonly HashSet<IUncommittedEvent> _uncommittedEvents =
            new HashSet<IUncommittedEvent>(new UncommittedEventEquality());

        private readonly object _lock = new object();
        private readonly IIdentity<TKey> _aggregateId;


        private AggregateRoot()
        {
        }

        /// <summary></summary>
        /// <param name="id"></param>
        protected AggregateRoot(IIdentity<TKey> id)
        {
            _aggregateId = id;
            Id = _aggregateId.Value ?? throw new NoNullAllowedException(
                $"The aggregateRoot can't accept null value for {nameof(AggregateId)}");
            // RegisterAppliers();
        }

        /// <summary>Gets  the primary key for this role.</summary>
        [JsonProperty]
        public TKey Id { get; private set; }

        /// <summary>_</summary>
        public IIdentity<TKey> AggregateId() => _aggregateId;


        /// <summary>_</summary>
        public IIdentity GetAggregateId() => _aggregateId;


        // /// <summary></summary>
        // protected abstract void RegisterAppliers();

        /// <summary>_</summary>
        protected abstract void EnsureValidState();

        /// <summary></summary>
        public Type GetAggregateType() => GetType();

        /// <summary></summary>
        public string AssemblyType => GetType().AssemblyQualifiedName ?? GetType().Name;

        /// <summary></summary>
        [JsonProperty]
        public int Version { get; private set; }

        /// <summary></summary>
        [JsonIgnore]
        protected string AggregateName => GetType().Name;

        /// <summary> </summary>
        [JsonIgnore]
        protected virtual bool IsNew => Version <= 0;

        /// <summary></summary>
        [JsonIgnore]
        protected bool IsCommitted => _uncommittedEvents.Count == 0;

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IAggregateRoot<TKey> UpdateId(IIdentity<TKey> value)
        {
            Id = value.Value;
            return this;
        }

        /// <summary></summary>
        [JsonIgnore]
        public IReadOnlyCollection<IUncommittedEvent> UncommittedEvents
        {
            get
            {
                try
                {
                    Monitor.Enter(_lock);
                    return _uncommittedEvents;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw new AggregateException(e.Message, e);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        // /// <summary></summary>
        // /// <param name="applier"></param>
        // /// <typeparam name="TAggregateEvent"></typeparam>
        // /// <exception cref="NotImplementedException"></exception>
        // public void RegisterApplier<TAggregateEvent>(Action<TAggregateEvent> applier)
        //     where TAggregateEvent : IAggregateEvent
        // {
        //     try
        //     {
        //         Monitor.Enter(_lock);
        //         _appliers.TryAdd(typeof(TAggregateEvent), e => applier((TAggregateEvent) e));
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw new AggregateException(e.Message, e);
        //     }
        //     finally
        //     {
        //         Monitor.Exit(_lock);
        //     }
        // }


        /// <summary>_</summary>
        /// <param name="event"></param>
        protected abstract void ApplyWhen(IAggregateEvent @event);

        protected virtual void UpdateMetadata(DomainMessageMetadata messageMetadata)
        {
        }

        /// <summary></summary>
        /// <param name="aggregateEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected void Apply(IAggregateEvent aggregateEvent)
        {
            if (aggregateEvent == null)
                throw new ArgumentNullException(nameof(aggregateEvent));
            
            var type = aggregateEvent.GetType();
            if (type.IsAbstract || !type.IsClass)
                throw new InvalidCastException($"Invalid aggregate event with type {type.Name} will to apply");

            ApplyWhen(aggregateEvent);
            EnsureValidState();

            var metadata = new DomainMessageMetadata()
                .SetMessageId(MessageId.FromType(GetAggregateType()))
                .SetOccurredOn(DateTimeConstants.UtcNow)
                .SetAggregateId(AggregateId())
                .SetAggregateName(AggregateName)
                .SetAggregateEventType(type);

            UpdateMetadata(metadata);
            if (metadata.HasValue(MetadataKeys.OriginalVersion))
                ValidateEventVersion(metadata.OriginalVersion);
           
            IncrementVersion(metadata);
            AddEvent(aggregateEvent, metadata);
        }

        // /// <summary>_</summary>
        // protected virtual void ApplyAllChanges()
        // {
        //     foreach (var applier in _appliers)
        //     {
        //         var type = applier.Key;
        //         if (!typeof(IAggregateEvent).IsAssignableFrom(type)) continue;
        //         var obj = type.CreateInstance() as IAggregateEvent;
        //         if (obj is null) continue;
        //         Emit(obj);
        //     }
        // }

        /// <summary></summary>
        /// <param name="expectedVersion"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void ValidateEventVersion(long expectedVersion)
        {
            try
            {
                Monitor.Enter(_lock);
                if (Version != expectedVersion)
                    throw new InvalidOperationException(
                        $@"Invalid version specified : expectedVersion = {Version}
                          but  originalVersion = {expectedVersion}.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new AggregateException(e.Message, e);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary> </summary>
        public void MarkAsCommitted()
        {
            try
            {
                Monitor.Enter(_lock);
                _uncommittedEvents.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new AggregateException(e.Message, e);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary>_</summary>
        /// <param name="handler"></param>
        /// <param name="event"></param>
        protected void ApplyTo(IInternalEventHandler handler, IAggregateEvent @event) => handler.Handle(@event);

        void IInternalEventHandler.Handle(IAggregateEvent @event) => ApplyWhen(@event);

        /// <summary> </summary>
        /// <param name="originalVersion"></param>
        /// <returns></returns>
        protected virtual long BuildValidVersion(long originalVersion)
        {
            originalVersion = originalVersion > 0 ? originalVersion : Version;
            return originalVersion;
        }

        // /// <summary>_</summary>
        // /// <param name="aggregateEvent"></param>
        // /// <exception cref="InvalidOperationException"></exception>
        // /// <exception cref="AggregateException"></exception>
        // protected virtual void ApplyChanges(IAggregateEvent aggregateEvent)
        // {
        //     try
        //     {
        //         Monitor.Enter(_lock);
        //         var eventType = aggregateEvent.GetType();
        //
        //         if (!_appliers.ContainsKey(eventType))
        //             throw new InvalidOperationException(
        //                 $"Aggregate '{AggregateName}' does not have an 'applier' that takes aggregate event '{eventType.Name}' as argument");
        //
        //         _appliers[eventType](aggregateEvent);
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw new AggregateException(e.Message, e);
        //     }
        //     finally
        //     {
        //         Monitor.Exit(_lock);
        //     }
        // }

        /// <summary>_</summary>
        /// <param name="messageMetadata"></param>
        /// <exception cref="AggregateException"></exception>
        protected virtual void IncrementVersion(DomainMessageMetadata messageMetadata)
        {
            try
            {
                Monitor.Enter(_lock);
                Version++;
                messageMetadata.SetAggregateVersion(Version);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new AggregateException(e.Message, e);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        private void AddEvent<TAggregateEvent>(TAggregateEvent aggregateEvent, DomainMessageMetadata messageMetadata)
            where TAggregateEvent : IAggregateEvent
        {
            try
            {
                Monitor.Enter(_lock);
                _uncommittedEvents.Add(new UncommittedEvent(aggregateEvent, messageMetadata));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new AggregateException(e.Message, e);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        public override string ToString()
            => $"{GetType().Name}/{Version}-{Id}";

        #region Value object

        /// <summary> </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (GetType() != obj.GetType()) return false;
            var value = (AggregateRoot<TKey>) obj;
            return Equals(value);
        }


        /// <summary> </summary>
        public bool Equals(AggregateRoot<TKey>? obj)
            => obj != null && GetType() == obj.GetType() && EqualityValues().SequenceEqual(obj.EqualityValues());

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
        public static bool operator ==(AggregateRoot<TKey>? left, AggregateRoot<TKey>? right)
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
        public static bool operator !=(AggregateRoot<TKey>? left, AggregateRoot<TKey>? right)
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