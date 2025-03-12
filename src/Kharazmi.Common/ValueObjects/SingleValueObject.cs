using System;
using System.Collections.Generic;
using System.Reflection;
using Kharazmi.Common.Extensions;
using Newtonsoft.Json;

namespace Kharazmi.Common.ValueObjects
{
    /// <summary>_</summary>
    public interface ISingleValueObject
    {
        /// <summary>_</summary>
        object GetValue();

        /// <summary> </summary>
        TValueKey ValueAs<TValueKey>() where TValueKey : IEquatable<TValueKey>, IComparable;

        /// <summary></summary>
        string ValueAsString();

        /// <summary> </summary>
        bool IsTransient();
    }

    /// <summary>_</summary>
    public interface ISingleValueObject<out TKey> : ISingleValueObject
        where TKey : IEquatable<TKey>, IComparable
    {
        /// <summary>_</summary>
        [JsonProperty]
        TKey Value { get; }
    }

    /// <summary>_</summary>
    public abstract class SingleValueObject<TKey> : ValueObject, IComparable, ISingleValueObject<TKey>
        where TKey : IComparable, IEquatable<TKey>
    {
        private static readonly Type Type = typeof(TKey);
        private static readonly TypeInfo TypeInfo = typeof(TKey).GetTypeInfo();

        /// <summary> </summary>
        [JsonProperty]
        public TKey Value { get; protected set; }

        /// <summary>_</summary>
        protected SingleValueObject(TKey value)
        {
            if (TypeInfo.IsEnum && !Enum.IsDefined(Type, value))
                throw new ArgumentException($"The value '{value}' isn't defined in enum '{Type.PrettyPrint()}'");

            Value = value;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public object GetValue() => Value;

        /// <summary>_</summary>
        /// <typeparam name="TValueKey"></typeparam>
        /// <returns></returns>
        public TValueKey ValueAs<TValueKey>() where TValueKey : IEquatable<TValueKey>, IComparable
            => Value.ChangeTypeTo<TValueKey>();


        /// <summary></summary>
        /// <returns></returns>
        public string ValueAsString() => Value.ChangeTypeTo<string>() ?? "";

        /// <summary>_</summary>
        /// <returns></returns>
        public virtual bool IsTransient()
        {
            if (EqualityComparer<TKey>.Default.Equals(Value, default)) return true;

            if (typeof(TKey) == typeof(int)) return Convert.ToInt32(Value) <= 0;
            if (typeof(TKey) == typeof(long)) return Convert.ToInt64(Value) <= 0;

            return false;
        }


        /// <summary>_</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
                throw new ArgumentNullException(nameof(obj));

            var other = obj as SingleValueObject<TKey>;
            if (other == null)
                throw new ArgumentException(
                    $"Cannot compare '{GetType().PrettyPrint()}' and '{obj.GetType().PrettyPrint()}'");

            return Value.CompareTo(other.Value);
        }

        /// <summary></summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static implicit operator TKey(SingleValueObject<TKey> self) => self.Value;

        /// <summary>_</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override IEnumerable<object> EqualityValues(object? obj)
        {
            yield return Value;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public override string ToString() => Value.ConvertToString() ?? "";
    }
}