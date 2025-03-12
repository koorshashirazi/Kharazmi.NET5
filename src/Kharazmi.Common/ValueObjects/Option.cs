using System;
using System.Collections.Generic;

namespace Kharazmi.Common.ValueObjects
{
    public abstract class Option<T>
    {
        public static implicit operator Option<T>(T value) =>
            new Some<T>(value);

        public static implicit operator Option<T>(None none) =>
            new None<T>();

        public abstract Option<TResult> Map<TResult>(Func<T, TResult> map);
        public abstract Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map);
        public abstract T Reduce(T whenNone);
        public abstract T Reduce(Func<T> whenNone);

        public Option<TNew> OfType<TNew>() where TNew : class =>
            this is Some<T> some && typeof(TNew).IsAssignableFrom(typeof(T))
                ? new Some<TNew>(some.Value as TNew)
                : (Option<TNew>) None.Value;
    }

    public sealed class Some<T> : Option<T>, IEquatable<Some<T>>
    {
        public T Value { get; }

        public Some(T value)
        {
            Value = value;
        }

        public static implicit operator T(Some<T> some) =>
            some.Value;

        public static implicit operator Some<T>(T value) =>
            new Some<T>(value);

        public override Option<TResult> Map<TResult>(Func<T, TResult> map) =>
            map(Value);

        public override Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map) =>
            map(Value);

        public override T Reduce(T whenNone) =>
            Value;

        public override T Reduce(Func<T> whenNone) =>
            Value;

        public override string ToString() =>
            $"Some({ContentToString})";

        private string ContentToString =>
            Value?.ToString() ?? "<null>";

        public bool Equals(Some<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Some<T> && Equals((Some<T>) obj);
        }

        public override int GetHashCode()
        {
            if (Value is null) return -1;
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public static bool operator ==(Some<T>? a, Some<T>? b) =>
            a is null && b is null ||
            !(a is null) && a.Equals(b);

        public static bool operator !=(Some<T>? a, Some<T>? b) => !(a == b);
    }

    public sealed class None<T> : Option<T>, IEquatable<None<T>>, IEquatable<None>
    {
        public override Option<TResult> Map<TResult>(Func<T, TResult> map) =>
            None.Value;

        public override Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map) =>
            None.Value;

        public override T Reduce(T whenNone) =>
            whenNone;

        public override T Reduce(Func<T> whenNone) =>
            whenNone();

        public override bool Equals(object? obj) =>
            !(obj is null) && ((obj is None<T>) || (obj is None));

        public override int GetHashCode() => 0;

        public bool Equals(None<T>? other) => true;

        public bool Equals(None? other) => true;

        public static bool operator ==(None<T>? a, None<T>? b) =>
            (a is null && b is null) ||
            (!(a is null) && a.Equals(b));

        public static bool operator !=(None<T>? a, None<T>? b) => !(a == b);

        public override string ToString() => "None";
    }

    public sealed class None : IEquatable<None>
    {
        public static None Value { get; } = new None();

        private None()
        {
        }

        public override string ToString() => "None";

        public override bool Equals(object? obj) =>
            !(obj is null) && (obj is None || IsGenericNone(obj.GetType()));

        private bool IsGenericNone(Type type) =>
            type.GenericTypeArguments.Length == 1 &&
            typeof(None<>).MakeGenericType(type.GenericTypeArguments[0]) == type;

        public bool Equals(None? other) => true;

        public override int GetHashCode() => 0;
    }
}