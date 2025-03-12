#region

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace Kharazmi.Common.Json
{
    /// <summary>_</summary>
    public static class ImmutableJson
    {
        /// <summary>_</summary>
        public static readonly JsonSerializerSettings DefaultJsonSettings;

        static ImmutableJson()
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
        }


        /// <summary>_</summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static string Serialize(this object jsonObject)
            => Serialize(jsonObject, DefaultJsonSettings);

        /// <summary>_</summary>
        /// <param name="jsonObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Serialize(this object jsonObject, Type type)
            => Serialize(jsonObject, type, DefaultJsonSettings);

        /// <summary>_</summary>
        /// <param name="jsonObject"></param>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Serialize(this object jsonObject, Type type, JsonSerializerSettings settings)
            => JsonConvert.SerializeObject(jsonObject, type, settings);

        /// <summary>_</summary>
        /// <param name="jsonObject"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Serialize(this object jsonObject, JsonSerializerSettings settings)
            => JsonConvert.SerializeObject(jsonObject, settings);

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static object? Deserialize(
            this string json,
            Type? type,
            JsonSerializerSettings settings)
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
        /// <returns></returns>
        public static object? Deserialize(this string json)
            => Deserialize(json, null, DefaultJsonSettings);

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object? Deserialize(this string json, Type? type)
            => Deserialize(json, type, DefaultJsonSettings);

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static object? Deserialize(this string json, JsonSerializerSettings settings)
            => Deserialize(json, null, settings);


        //// <summary>_</summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(this string json)
            => Deserialize<T>(json, DefaultJsonSettings);


        public static T Deserialize<T>(this StreamReader reader) where T : class
            => JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;

        /// <summary>_</summary>
        /// <param name="json"></param>
        /// <param name="settings"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(this string json, JsonSerializerSettings? settings)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json, settings);
                return obj ?? JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings());
            }
            catch
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}