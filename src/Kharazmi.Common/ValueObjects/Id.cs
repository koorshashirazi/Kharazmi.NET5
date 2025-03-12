#region

using System;
using Kharazmi.Common.Extensions;
using Kharazmi.Common.Json;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.ValueObjects
{
    /// <summary> </summary>
    /// <typeparam name="TKey"></typeparam>
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class Id<TKey> : Identity<Id<TKey>, TKey>
        where TKey : IEquatable<TKey>, IComparable
    {
        /// <summary></summary>
        /// <param name="value"></param>
        [JsonConstructor]
        public Id(TKey value) : base(value)
        {
        }

        /// <summary>
        /// Return new id with default value of key
        /// </summary>
        [JsonIgnore]
        public static Id<TKey> Empty
        {
            get
            {
                var keyType = typeof(TKey);

                if (keyType == typeof(Guid))
                    return new Id<Guid>(Guid.Empty).ChangeTypeTo<TKey>();

                return Type.GetTypeCode(keyType) switch
                {
                    TypeCode.SByte => new Id<sbyte>(default).ChangeTypeTo<TKey>(),
                    TypeCode.UInt16 => new Id<ushort>(default).ChangeTypeTo<TKey>(),
                    TypeCode.UInt32 => new Id<uint>(default).ChangeTypeTo<TKey>(),
                    TypeCode.UInt64 => new Id<ulong>(default).ChangeTypeTo<TKey>(),
                    TypeCode.Int16 => new Id<short>(default).ChangeTypeTo<TKey>(),
                    TypeCode.Int32 => new Id<int>(default).ChangeTypeTo<TKey>(),
                    TypeCode.Int64 => new Id<long>(default).ChangeTypeTo<TKey>(),
                    TypeCode.Double => new Id<double>(default).ChangeTypeTo<TKey>(),
                    TypeCode.String => new Id<string>(string.Empty).ChangeTypeTo<TKey>(),
                    _ => throw new InvalidCastException($"Id can't create new key, invalid type of key {keyType}")
                };
            }
        }

        /// <summary>
        /// Return new id with value
        /// </summary>
        [JsonIgnore]
        public static Id<TKey> New
        {
            get
            {
                var keyType = typeof(TKey);

                if (keyType == typeof(string))
                    return new Id<string>(Guid.NewGuid().ToString("N")).ChangeTypeTo<TKey>();

                if (keyType == typeof(Guid))
                    return new Id<Guid>(Guid.NewGuid()).ChangeTypeTo<TKey>();

                return Type.GetTypeCode(keyType) switch
                {
                    TypeCode.SByte => new Id<sbyte>(1).ChangeTypeTo<TKey>(),
                    TypeCode.UInt16 => new Id<ushort>(1).ChangeTypeTo<TKey>(),
                    TypeCode.UInt32 => new Id<uint>(1).ChangeTypeTo<TKey>(),
                    TypeCode.UInt64 => new Id<ulong>(1).ChangeTypeTo<TKey>(),
                    TypeCode.Int16 => new Id<short>(1).ChangeTypeTo<TKey>(),
                    TypeCode.Int32 => new Id<int>(1).ChangeTypeTo<TKey>(),
                    TypeCode.Int64 => new Id<long>(1).ChangeTypeTo<TKey>(),
                    TypeCode.Double => new Id<double>(1).ChangeTypeTo<TKey>(),
                    TypeCode.String => new Id<string>(Guid.NewGuid().ToString("N")).ChangeTypeTo<TKey>(),
                    _ => throw new InvalidCastException($"Id can't create new key, invalid type of key {keyType}")
                };
            }
        }

        /// <summary>
        /// Increment value of id
        /// </summary>
        /// <exception cref="InvalidCastException"></exception>
        public void Increment()
        {
            var keyType = typeof(TKey);

            if (keyType == typeof(string))
            {
                Value = Guid.NewGuid().ChangeTypeTo<TKey>();
                return;
            }

            if (keyType == typeof(Guid))
            {
                Value = Guid.NewGuid().ChangeTypeTo<TKey>();
                return;
            }

            switch (Type.GetTypeCode(keyType))
            {
                case TypeCode.UInt16:
                    var ushortValue = Value.ChangeTypeTo<ushort>() + 1;
                    if (ushortValue >= ushort.MaxValue) ushortValue = 0;
                    Value = ushortValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.UInt32:
                    var uintValue = Value.ChangeTypeTo<uint>() + 1;
                    if (uintValue >= uint.MaxValue) uintValue = 0;
                    Value = uintValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.UInt64:
                    var ulongValue = Value.ChangeTypeTo<ulong>() + 1;
                    if (ulongValue >= ulong.MaxValue) ulongValue = 0;
                    Value = ulongValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.Int16:
                    var shortValue = Value.ChangeTypeTo<short>() + 1;
                    if (shortValue >= short.MaxValue) shortValue = 0;
                    Value = shortValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.Int32:
                    var intValue = Value.ChangeTypeTo<int>() + 1;
                    if (intValue >= int.MaxValue) intValue = 0;
                    Value = intValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.Int64:
                    var longValue = Value.ChangeTypeTo<long>() + 1;
                    if (longValue >= long.MaxValue) longValue = 0;
                    Value = longValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.Double:
                    var doubleValue = Value.ChangeTypeTo<double>() + 1;
                    if (doubleValue >= double.MaxValue) doubleValue = 0;
                    Value = doubleValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.SByte:
                    var sbyteValue = Value.ChangeTypeTo<sbyte>() + 1;
                    if (sbyteValue >= sbyte.MaxValue) sbyteValue = 0;
                    Value = sbyteValue.ChangeTypeTo<TKey>();
                    return;
                case TypeCode.String:
                    Value = Guid.NewGuid().ToString("N").ChangeTypeTo<TKey>();
                    break;
                default:
                    throw new InvalidCastException($"Id can't increment value id, invalid type of key {keyType}");
            }
        }

        /// <summary> </summary>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static Id<TKey> FromId(IIdentity<TKey> value) => FromValue(value.ValueAs<TKey>());

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Id<TKey> FromValue(TKey value) => new Id<TKey>(value);

        /// <summary> </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Id<Guid> FromGuid(Guid value) => new Id<Guid>(value);

        /// <summary> </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Id<string> FromString(string value) => new Id<string>(value);

        /// <summary></summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static implicit operator TKey(Id<TKey> self) => self.Value;

        /// <summary>_</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static implicit operator Id<TKey>(TKey key) => new Id<TKey>(key);
    }
}