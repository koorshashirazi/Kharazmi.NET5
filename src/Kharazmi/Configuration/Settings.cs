using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Kharazmi.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kharazmi.Configuration
{
    public abstract class Settings : Dictionary<string, object>, ISettings
    {
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        protected static IEnumerable<PropertyInfo> PropertyInfos;

        private bool _isDirty;

        static Settings()
        {
            PropertyInfos = new List<PropertyInfo>();
        }

        protected Settings()
        {
            PropertyInfos = CurrentType.GetSettingsProperties();
        }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        protected Type CurrentType => GetType();

        public bool IsDirty() => _isDirty;

        public void MakeDirty(bool value) => _isDirty = value;

        public object GetOption(Type optionType)
        {
            TryGetValue(optionType.Name, out var value);
            return value switch
            {
                JObject jObject => jObject.ToObject<object>() ??
                                   throw new NotFoundOptionException(optionType.Name, optionType.Name),
                IOptions obj => obj,
                _ => throw new NotFoundOptionException(optionType.Name, optionType.Name),
            };
        }

        public object? TryGetOption(Type optionType)
        {
            try
            {
                TryGetValue(optionType.Name, out var value);
                return value switch
                {
                    JObject jObject => jObject.ToObject<object>() ??
                                       throw new NotFoundOptionException(optionType.Name, optionType.Name),
                    IOptions obj => obj,
                    _ => default
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default;
            }
        }

        public T GetOption<T>() where T : IOptions
        {
            TryGetValue(typeof(T).Name, out var value);
            return value switch
            {
                JObject jObject => jObject.ToObject<T>() ??
                                   throw new NotFoundOptionException(typeof(T).Name, typeof(T).Name),
                T option => option,
                _ => throw new NotFoundOptionException(typeof(T).Name, typeof(T).Name)
            };
        }

        public T? TryGetOption<T>() where T : IOptions
        {
            try
            {
                TryGetValue(typeof(T).Name, out var value);
                return value switch
                {
                    JObject jObject => jObject.ToObject<T>(),
                    T option => option,
                    _ => default
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default;
            }
        }

        public void AddOption(string key, object value)
        {
            _isDirty = true;
            TryAdd(key, value);
        }

        public void AddOption<T>(T value) where T : class, IOptions
        {
            _isDirty = true;
            TryAdd(value.GetType().Name, value);
        }

        public void UpdateOption<T>(T option) where T : class, IOptions
        {
            _isDirty = true;
            option.MakeDirty(true);
            Remove(option.GetType().Name);
            TryAdd(option.GetType().Name, option);
        }
        

        protected ISettings UpdatePropertiesValue([MaybeNull] ISettings? appSettings)
        {
            if (appSettings is null) return this;
            var props = CurrentType.GetProperties().Where(x => x.CanWrite);

            foreach (var prop in props)
            {
                var value = prop.GetValue(appSettings);
                if (value is null) continue;
                prop.SetValue(this, value);
            }

            return this;
        }

        protected object GetPropertyValue([NotNull] string path)
        {
            try
            {
                return ObjectExtensions.GetPropertyValue(this, path) ?? throw new InvalidOperationException();
            }
            catch (Exception)
            {
                try
                {
                    var jObject = JObject.FromObject(this, JsonSerializer.Create(GetJsonSettings));
                    var jToken = jObject[path];
                    return jToken?.ToObject<object>() ?? throw new InvalidOperationException();
                }
                catch
                {
                    throw new KeyNotFoundException(
                        $"Can't find section {path} from configuration. Make sure you register it!");
                }
            }
        }

        protected T GetPropertyValue<T>([NotNull] string path) where T : class
            => (T) GetPropertyValue(path);

        protected object? TryGetPropertyValue([NotNull] string path)
        {
            try
            {
                return ObjectExtensions.GetPropertyValue(this, path);
            }
            catch (Exception)
            {
                try
                {
                    var jObject = JObject.FromObject(this, JsonSerializer.Create(GetJsonSettings));
                    var jToken = jObject[path];
                    return jToken?.ToObject<object>();
                }
                catch
                {
                    return default;
                }
            }
        }

        protected T? TryGetPropertyValue<T>([NotNull] string path) where T : class
        {
            try
            {
                return ObjectExtensions.GetPropertyValue(this, path) as T;
            }
            catch
            {
                try
                {
                    var jObject = JObject.FromObject(this, JsonSerializer.Create(GetJsonSettings));
                    var jToken = jObject[path];
                    return jToken?.ToObject<T>();
                }
                catch
                {
                    return default;
                }
            }
        }

        protected ISettings TrySetPropertyValue<TSettings>([NotNull] string path, [NotNull] object value)
            where TSettings : ISettings
        {
            value.NotNull(nameof(value));

            try
            {
                this.SetPropertyValue(path, value);
                return this;
            }
            catch
            {
                try
                {
                    var jObject = JObject.FromObject(this, JsonSerializer.Create(GetJsonSettings));
                    jObject[path] = JObject.FromObject(value, JsonSerializer.Create(GetJsonSettings));
                    var thisObject = jObject.ToObject<TSettings>();
                    var propObject = ObjectExtensions.GetPropertyValue(thisObject, path);
                    var ignoreProps = GetJsonIgnoreProp(value);

                    foreach (var (propKey, propValue) in ignoreProps)
                    {
                        var prop = propObject?.GetType().GetProperty(propKey);
                        prop?.SetValue(propObject, propValue);
                    }

                    UpdatePropertiesValue(thisObject);
                    return this;
                }
                catch
                {
                    return this;
                }
            }
        }

        protected ISettings TrySetPropertyValue<TSettings, T>([NotNull] string path, [NotNull] T value)
            where TSettings : ISettings
            where T : class
        {
            try
            {
                value.NotNull(nameof(value));
                this.SetPropertyValue(path, value);
                return this;
            }
            catch
            {
                try
                {
                    var jObject = JObject.FromObject(this, JsonSerializer.Create(GetJsonSettings));
                    jObject[path] = JObject.FromObject(value, JsonSerializer.Create(GetJsonSettings));
                    var thisObject = jObject.ToObject<TSettings>();
                    var propObject = thisObject.GetPropertyValue(path);
                    var ignoreProps = GetJsonIgnoreProp(value);

                    foreach (var (propKey, propValue) in ignoreProps)
                    {
                        var prop = propObject?.GetType().GetProperty(propKey);
                        prop?.SetValue(propObject, propValue);
                    }

                    UpdatePropertiesValue(thisObject);
                    return this;
                }
                catch
                {
                    return this;
                }
            }
        }

        protected virtual PropertyInfo? Find<T>()
            => CurrentType.GetProperties().FirstOrDefault(prop => typeof(T) == prop.PropertyType);

        public static JsonSerializerSettings GetJsonSettings => new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            MaxDepth = 50,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double,
            MissingMemberHandling = MissingMemberHandling.Error,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DefaultValueHandling = DefaultValueHandling.Include,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            }
        };

        private static Dictionary<string, object?> GetJsonIgnoreProp(object? src)
        {
            var dic = new Dictionary<string, object?>();
            if (src is null) return dic;
            var props = src.GetType().GetProperties();
            foreach (var prop in props)
            {
                var jsonIgnore = prop.GetCustomAttribute(typeof(JsonIgnoreAttribute));
                if (jsonIgnore is null) continue;
                dic.TryAdd(prop.Name, prop.GetValue(src));
            }

            return dic;
        }
    }
}