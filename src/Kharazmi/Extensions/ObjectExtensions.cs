#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Runtime;
using Newtonsoft.Json;
using Serializer = Kharazmi.Json.Serializer;

#endregion

namespace Kharazmi.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull([NotNullWhen(false)] this object? value)
            => value is null;

        public static Dictionary<string, TValue>? ToDictionary<TValue>([NotNull] this object obj)
        {
            var json = Serializer.Serialize(obj);
            var dictionary = Serializer.Deserialize<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public static T ToObject<T>([NotNull] this IDictionary<string, object> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
                someObjectType
                    .GetProperty(item.Key)
                    ?.SetValue(someObject, item.Value, null);

            return someObject;
        }

        public static Dictionary<string, object?> AsDictionary([NotNull] this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }


        /// <summary>
        /// Converts given object to a value or enum type using <see cref="Convert.ChangeType(object,TypeCode)"/> or <see cref="Enum.Parse(Type,string)"/> method.
        /// </summary>
        /// <param name="value">Object to be converted</param>
        /// <typeparam name="T">Type of the target object</typeparam>
        /// <returns>Converted object</returns>
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

        public static T? ConvertToNumber<T>(this object? value)
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

        /// <summary>
        /// Converts the provided <paramref name="value"/> to a strongly typed value object.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>An instance of <typeparamref name="T"/> representing the provided <paramref name="value"/>.</returns>
        public static T? FromString<T>([NotNull] this string value)
        {
            return ChangeTypeTo<T>(value);
        }


        /// <summary>
        ///     Check if an item is in a list.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="list">List of items</param>
        /// <typeparam name="T">Type of the items</typeparam>
        public static bool IsIn<T>(this T item, params T[] list)
            => item != null && list.Contains(item);

        /// <summary>
        /// Converts the provided <paramref name="value"/> to a strongly typed value object.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>An instance of <typeparamref name="T"/> representing the provided <paramref name="value"/>.</returns>
        public static T? ChangeTypeTo<T>([NotNull] this string value)
            => FromString<T>(value);

        /// <summary>_</summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static string GetGenericTypeName(this object @object)
            => @object.GetType().GetTypeInfo().GetGenericTypeName();

        public static byte[] ToBytes<T>(this T obj) where T : class
        {
            var json = Serializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T? FromBytes<T>([NotNull] this byte[] data) where T : class
        {
            using var stream = new MemoryStream(data);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return Serializer.Deserialize<T>(reader);
        }

        public static bool IsNumericType([NotNull] this object o)
        {
            return Type.GetTypeCode(o.GetType()) switch
            {
                TypeCode.Byte => true,
                TypeCode.SByte => true,
                TypeCode.UInt16 => true,
                TypeCode.UInt32 => true,
                TypeCode.UInt64 => true,
                TypeCode.Int16 => true,
                TypeCode.Int32 => true,
                TypeCode.Int64 => true,
                TypeCode.Decimal => true,
                TypeCode.Double => true,
                TypeCode.Single => true,
                _ => false
            };
        }

        public static T? MapTo<T>(this object? value, bool ignoreNull = true) where T : class
        {
            if (value is null) return default;

            try
            {
                var settings = Serializer.DefaultJsonSettings;
                if (ignoreNull)
                    settings.NullValueHandling = NullValueHandling.Ignore;

                var json = Serializer.Serialize(value, settings);
                var config = Serializer.Deserialize<T>(json, settings);
                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Message: {0}", e.Message);
                throw new InvalidCastException(e.Message);
            }
        }

        public static object? GetPropertyValue(this object? src, string? propName, int counter = 0)
        {
            while (true)
            {
                if (src is null || propName.IsEmpty()) return default;

                if (propName.Contains("."))
                {
                    var temp = propName.Split(new[] {'.'}, 5);
                    src = GetPropertyValue(src, temp[counter], counter);
                    counter += 1;
                    propName = temp[counter];
                    continue;
                }

                var prop = src.GetType().GetProperty(propName);

                return prop != null ? prop.GetValue(src, null) : null;
            }
        }

        public static PropertyInfo? GetPropertyInfo(this object? src, string? propName, int counter = 0)
        {
            while (true)
            {
                if (src is null || propName.IsEmpty()) return default;

                if (!propName.Contains(".")) return src.GetType().GetProperty(propName);
                var temp = propName.Split(new[] {'.'}, 5);
                src = GetPropertyInfo(src, temp[counter], counter);
                counter += 1;
                propName = temp[counter];
            }
        }

        public static object? SetPropertyValue(this object? src, string? propName, object newValue, int counter = 0)
        {
            while (true)
            {
                if (src is null || propName.IsEmpty()) return default;

                if (propName.Contains("."))
                {
                    var temp = propName.Split(new[] {'.'}, 5);
                    src = SetPropertyValue(src, temp[counter], newValue, counter);
                    counter += 1;
                    propName = temp[counter];
                    continue;
                }

                var prop = src.GetType().GetProperty(propName);

                if (prop != null && prop.PropertyType == newValue.GetType())
                {
                    prop.SetValue(src, newValue);
                }

                return prop != null ? prop.GetValue(src, null) : null;
            }
        }

        public static void Merge<T>(this T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }

        public static MetadataCollection ToMetadata([NotNull] this object obj)
        {
            var json = Serializer.Serialize(obj);
            var dictionary = Serializer.Deserialize<MetadataCollection>(json);
            return dictionary ?? new MetadataCollection();
        }

        public static IEnumerable<Property> GetAllPropertyValue(this object obj, Type objType,
            bool withBase = false)
        {
            var props = new List<Property>();
            obj.GetAllPropertyValue(objType, props, withBase);
            return props;
        }

        private static void GetAllPropertyValue(this object? obj, Type objType,
            ICollection<Property> result, bool withBase, bool isGeneric = false, string? path = null,
            string[]? arguments = null)
        {
            // todo Optimize using delegation and runTime cache 
            if (obj is null || obj.GetType().IsPrimitive) return;
            try
            {
                PropertyInfo[] properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo property in properties)
                {
                    var propDeclaringType = property.DeclaringType;
                    if (propDeclaringType != objType && !withBase) continue;

                    path = string.IsNullOrWhiteSpace(path) ? $"{propDeclaringType?.Name}" : path;

                    var propValue = property.GetValue(obj, null);
                    if (propValue is null) continue;

                    var propType = property.PropertyType;
                    if (propType.IsArray)
                    {
                        var subPropValues = propValue as IEnumerable;
                        var sourceObjectType = objType;
                        var currentPath = path;
                        foreach (var item in subPropValues)
                            GetAllPropertyValue(item, objType, result, withBase, false, path);

                        objType = sourceObjectType;
                        path = currentPath;
                    }

                    if (propType.IsGenericType)
                    {
                        var argTyp1 = propType.GetGenericArguments().FirstOrDefault();
                        if (argTyp1 is null) continue;

                        var enumerableType = typeof(IEnumerable<>).MakeGenericType(argTyp1);
                        if (enumerableType.IsAssignableFrom(propType))
                        {
                            var subPropValues = propValue as IEnumerable;
                            var sourceObjectType = objType;
                            var currentPath = path;
                            foreach (var item in subPropValues)
                                GetAllPropertyValue(item, objType, result, withBase, true, $"{path}:{property.Name}");

                            objType = sourceObjectType;
                            path = currentPath;
                        }

                        var argTyp2 = propType.GetGenericArguments().LastOrDefault();
                        if (argTyp2 is null) continue;

                        var dicType = typeof(IDictionary<,>).MakeGenericType(argTyp1, argTyp2);
                        if (dicType.IsAssignableFrom(propType))
                        {
                            var subPropValues = propValue as IDictionary;
                            var sourceObjectType = objType;
                            var currentPath = path;
                            foreach (dynamic item in subPropValues)
                                GetAllPropertyValue(item.Value, objType, result, withBase, true,
                                    $"{path}:{property.Name}:" + item.Key);

                            objType = sourceObjectType;
                            path = currentPath;
                        }
                    }
                    else
                    {
                        if (propValue.GetType().IsValidPrimaryType())
                        {
                            result.Add(Property.For(propValue, property.Name, path, propType.Name, objType.Name,
                                isGeneric, arguments));
                        }
                        else if (propValue is ISingleValueObject singleValueObject)
                        {
                            result.Add(Property.For(singleValueObject.GetValue(), property.Name, path, propType.Name,
                                objType.Name, isGeneric, arguments));
                        }
                        else if (property.PropertyType.Assembly == objType.Assembly && !property.PropertyType.IsArray)
                        {
                            var sourceObjectType = objType;
                            var currentPath = path;
                            GetAllPropertyValue(propValue, objType, result, withBase, false, $"{path}:{property.Name}");
                            objType = sourceObjectType;
                            path = currentPath;
                        }
                        else if (!property.PropertyType.IsAbstract && property.PropertyType.IsClass)
                        {
                            var sourceObjectType = objType;
                            var currentPath = path;
                            GetAllPropertyValue(propValue, property.PropertyType, result, withBase, false,
                                $"{path}:{property.Name}");
                            objType = sourceObjectType;
                            path = currentPath;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region Helper

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

        #endregion
    }
}