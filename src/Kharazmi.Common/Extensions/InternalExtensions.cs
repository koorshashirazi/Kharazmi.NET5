using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Extensions
{
    internal static class InternalExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> PrettyPrintCache =
            new ConcurrentDictionary<Type, string>();

        public static string ToUnderscore(this string value, char separator = '.')
        {
            var str1 = value.Split(separator);
            var str2 = str1.Select(s => s.ToLower().ToLowerWithFirstUpper());
            var str3 = string.Concat(str2).Underscore();
            return str3;
        }

        public static string Underscore(this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()));

        public static string ToLowerWithFirstUpper(this string input)
            => input.IsNotEmpty() ? input.First().ToString().ToUpper() + input.Substring(1).ToLower() : "";

        public static string FirstToUpper(this string input) => input.First().ToString().ToUpper() + input.Substring(1);
        public static string FirstToLower(this string input) => input.First().ToString().ToLower() + input.Substring(1);

        public static bool IsEmpty(this string value)
            => string.IsNullOrWhiteSpace(value);

        public static bool IsNotEmpty(this string value)
            => !IsEmpty(value);

        public static string UniqueTypeName(this Type type, char separator = ':')
        {
            if (type == null) return "";
            var name = type.Name.Underscore();
            var prefixName = type.Namespace?.ToUnderscore();
            return $"{prefixName}{separator}{name}".ToLowerInvariant();
        }

        public static string CacheKey(this Type type, string key, char separator = ':')
            => $"{type.Name}{separator}{key}";

        public static T CreateInstance<T>(this Type type, params object[] parameters) where T : class
        {
            try
            {
                return Activator.CreateInstance<T>();
            }
            catch
            {
                try
                {
                    return (T) CreateInstance(type, parameters);
                }
                catch (Exception e)
                {
                    var instance = CreateInstance(type, parameters) as T;
                    if (instance is null == false) return instance;
                    Console.WriteLine(e);
                    throw new InvalidCastException("Can't cast a instance object to type {typeof(T).Name}");
                }
            }
        }

        public static object CreateInstance(this Type type, params object[] parameters)
        {
            var args = parameters.ToArray();
            object? instance;
            try
            {
                instance = Activator.CreateInstance(type, BindingFlags.NonPublic, null, args,
                    System.Threading.Thread.CurrentThread.CurrentCulture);
                if (instance is null)
                    throw new TypeAccessException(type.Name);
                return instance;
            }
            catch
            {
                try
                {
                    instance = Activator.CreateInstance(type,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args,
                        System.Threading.Thread.CurrentThread.CurrentCulture);
                    if (instance is null)
                        throw new TypeAccessException(type.Name);
                    return instance;
                }
                catch
                {
                    try
                    {
                        instance = Activator.CreateInstance(type, true);
                        if (instance is null)
                            throw new TypeAccessException(type.Name);
                        return instance;
                    }
                    catch
                    {
                        throw new TypeAccessException(type.Name);
                    }
                }
            }
        }


        public static T ChangeTypeTo<T>(this object value)
        {
            var valueType = typeof(T);

            if (value == null) throw new NoNullAllowedException($"Can't change type to {valueType}, Value is null");

            if (valueType == typeof(string))
            {
                var strValue = ConvertToString(value);
                if (strValue == null || strValue.IsEmpty())
                    throw new InvalidCastException("The received value object can't convert to string");

                var strObj = TypeDescriptor.GetConverter(valueType).ConvertFromInvariantString(strValue);
                if (strObj is T strObject) return strObject;
            }

            if (valueType == typeof(Guid))
            {
                var guidValue = ConvertToString(value);
                if (guidValue is null)
                    throw new InvalidCastException("The received value object can't convert to Guid");
                var guid = TypeDescriptor.GetConverter(valueType).ConvertFromInvariantString(guidValue);
                if (guid is T guidObject) return guidObject;
            }

            var number = ConvertToNumber<T>(value);
            if (!(number is null)) return number;

            if (!valueType.IsEnum) return (T) Convert.ChangeType(value, valueType, CultureInfo.InvariantCulture);

            if (!Enum.IsDefined(valueType, value)) throw new InvalidCastException($"Enum type undefined '{value}'.");

            var enumValue = ConvertToString(value);
            if (enumValue is null)
                throw new InvalidCastException("The received value object can't convert to Enum");
            return (T) Enum.Parse(valueType, enumValue);
        }

        public static string? ConvertToString(this object? value)
        {
            if (value == null) return "";
            if (value is long l) return ToString(l);
            if (value is int i) return ToString(i);
            if (value is float f) return ToString(f);
            if (value is double d) return ToString(d);
            if (value is ulong ul) return ToString(ul);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static T ConvertToNumber<T>(this object? value)
        {
            try
            {
                if (typeof(T) == typeof(int)) return (T) (object) Convert.ToInt32(value);
                if (typeof(T) == typeof(long)) return (T) (object) Convert.ToInt64(value);
                if (typeof(T) == typeof(short)) return (T) (object) Convert.ToInt16(value);
                if (typeof(T) == typeof(float)) return (T) (object) Convert.ToSingle(value);
                if (typeof(T) == typeof(double)) return (T) (object) Convert.ToDouble(value);
                if (typeof(T) == typeof(decimal)) return (T) (object) Convert.ToDecimal(value);
                if (typeof(T) == typeof(uint)) return (T) (object) Convert.ToUInt32(value);
                if (typeof(T) == typeof(ulong)) return (T) (object) Convert.ToUInt64(value);
                if (typeof(T) == typeof(ushort)) return (T) (object) Convert.ToUInt16(value);
                if (typeof(T) == typeof(sbyte)) return (T) (object) Convert.ToSByte(value);
            }
            catch
            {
                // Ignored
            }

            return default;
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }

        public static TArgument IsNotNull<TArgument>(this TArgument? argument, string paramName)
            where TArgument : class
        {
            return argument ?? throw new ArgumentNullException(paramName);
        }

        public static string IsNotEmpty(this string? argument, string parameterName)
        {
            return string.IsNullOrEmpty(argument)
                ? throw new ArgumentNullException(parameterName)
                : argument;
        }

        private static string ToString(long value) => value.ToString(NumberFormatInfo.InvariantInfo);

        private static string ToString(ulong value) => value.ToString(NumberFormatInfo.InvariantInfo);

        private static string ToString(double value)
        {
            if (double.IsInfinity(value))
            {
                if (double.IsPositiveInfinity(value)) return "+inf";
                if (double.IsNegativeInfinity(value)) return "-inf";
            }

            return value.ToString("G17", NumberFormatInfo.InvariantInfo);
        }

        public static string PrettyPrint(this Type type)
        {
            return PrettyPrintCache.GetOrAdd(type, t =>
            {
                try
                {
                    return PrettyPrintRecursive(t, 0);
                }
                catch (Exception)
                {
                    return t.Name;
                }
            });
        }

        private static string PrettyPrintRecursive(Type type, int depth)
        {
            if (depth > 3) return type.Name;

            var nameParts = type.Name.Split('`');
            if (nameParts.Length == 1) return nameParts[0];

            var genericArguments = type.GetTypeInfo().GetGenericArguments();
            return !type.IsConstructedGenericType
                ? $"{nameParts[0]}<{new string(',', genericArguments.Length - 1)}>"
                : $"{nameParts[0]}<{string.Join(",", genericArguments.Select(t => PrettyPrintRecursive(t, depth + 1)))}>";
        }

        /// <summary>_</summary>
        public static T As<T>(this IIdentity value) where T : IIdentity
            => (T) value;

        /// <summary>
        ///  Used to simplify and beautify casting an object to a type.
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static T As<T, TKey>(this IIdentity<TKey> value)
            where T : IIdentity<TKey> where TKey : IEquatable<TKey>, IComparable => (T) value;

        public static bool IsNull(this object? obj)
            => obj is null;

        public static bool IsNotNull(this object? obj)
            => !(obj is null);
    }

    public static class PrefixExtensions
    {
        public static string WithNullPrefix(this string value)
        {
            return value.Insert(0, "#Null- ");
        }
    }
}