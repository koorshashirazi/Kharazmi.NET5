#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

#endregion

namespace Kharazmi.Json
{
    /// <summary>_</summary>
    public static class Serializer
    {
        /// <summary>_</summary>
        public static readonly JsonSerializerSettings DefaultJsonSettings;

        public static readonly JsonSerializerOptions DefaultJsonNetSettings;

        static Serializer()
        {
            DefaultJsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new ConstructorResolver()
            };
            DefaultJsonSettings.Converters.Add(new StringEnumConverter());
            DefaultJsonSettings.Converters.Add(new IsoDateTimeConverter());
            DefaultJsonSettings.Converters.Add(new KeyValuePairConverter());

            DefaultJsonNetSettings = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
            };
            DefaultJsonNetSettings.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>_</summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static string Serialize([NotNull] this object jsonObject)
            => Serialize(jsonObject, DefaultJsonSettings);

        /// <summary>_</summary>
        /// <param name="jsonObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Serialize([NotNull] this object jsonObject, Type type)
            => Serialize(jsonObject, type, DefaultJsonSettings);

        public static string Serialize([NotNull] this object jsonObject, Type type, JsonSerializerSettings settings)
            => JsonConvert.SerializeObject(jsonObject, type, settings);

        public static string Serialize([NotNull] this object jsonObject, JsonSerializerSettings settings)
            => JsonConvert.SerializeObject(jsonObject, settings);

        public static object? Deserialize(
            [NotNull] this string json,
            Type? type,
            [NotNull] JsonSerializerSettings settings)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type, settings);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Message: {0}", e.Message);
                try
                {
                    return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings());
                }
                catch
                {
                    return default;
                }
            }
        }

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object? Deserialize([NotNull] this string json)
            => Deserialize(json, null, DefaultJsonSettings);

        public static object? Deserialize([NotNull] this string json, Type? type)
            => Deserialize(json, type, DefaultJsonSettings);

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <param name="settings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object? Deserialize([NotNull] this string json, [NotNull] JsonSerializerSettings settings)
            => Deserialize(json, null, settings);


        //// <summary>_</summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? Deserialize<T>([NotNull] this string json)
            => Deserialize<T>(json, DefaultJsonSettings);


        public static T? Deserialize<T>([NotNull] this StreamReader reader) where T : class
            => JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <param name="settings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? Deserialize<T>([NotNull] this string json, JsonSerializerSettings? settings)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json, settings);
                return obj ?? JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings());
            }
            catch
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(json, DefaultJsonNetSettings);
                }
            }
        }
    }
}