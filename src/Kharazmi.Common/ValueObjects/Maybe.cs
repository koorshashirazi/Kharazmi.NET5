using System;

namespace Kharazmi.Common.ValueObjects
{
    /// <summary></summary>
    public struct Maybe<T> : IEquatable<Maybe<T>>
        where T : class
    {
        private readonly T? _value;

        private Maybe(T? value)
            => _value = value;

        /// <summary></summary>
        public bool HasValue => _value != null;

        /// <summary></summary>
        public T Value => _value ?? throw new NullReferenceException(nameof(Value));

        /// <summary></summary>
        public static Maybe<T> None => new Maybe<T>();

        /// <summary></summary>
        public static implicit operator Maybe<T>(T value)
            => new Maybe<T>(value);

        /// <summary></summary>
        public static bool operator ==(Maybe<T> maybe, T value)
            => maybe.HasValue && maybe.Value.Equals(value);

        /// <summary></summary>
        public static bool operator !=(Maybe<T> maybe, T value)
            => !(maybe == value);

        /// <summary></summary>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
            => left.Equals(right);

        /// <summary></summary>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
            => !(left == right);

        /// <inheritdoc />
        /// <summary> Avoid boxing and Give type safety </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Maybe<T> other)
        {
            if (!HasValue && !other.HasValue)
                return true;

            if (!HasValue || !other.HasValue)
                return false;

            return _value != null && _value.Equals(other.Value);
        }

        /// <summary> Avoid reflection </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is T typed)
            {
                obj = new Maybe<T>(typed);
            }

            if (!(obj is Maybe<T> other)) return false;

            return Equals(other);
        }

        /// <summary> Good practice when overriding Equals method.
        /// If x.Equals(y) then we must have x.GetHashCode()==y.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => _value != null ? _value.GetHashCode() : default;

        /// <summary></summary>
        public override string ToString()
            => _value != null ? _value.ToString() ?? "NO VALUE" : "NO VALUE";
    }
}