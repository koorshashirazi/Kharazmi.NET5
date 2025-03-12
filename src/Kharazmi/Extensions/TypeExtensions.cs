#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Exceptions;
using Kharazmi.Models;
using Kharazmi.Options;
using Kharazmi.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serializer = Kharazmi.Json.Serializer;

#endregion

namespace Kharazmi.Extensions
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> PrettyPrintCache = new();
        private static readonly ConcurrentDictionary<Type, bool> IsSimpleTypeCache = new();

        public static string GetGenericTypeName(this Type type, bool withTypeName = true)
        {
            string? typeName = null;
            var attribute = type.GetCustomAttribute<DisplayNameAttribute>();

            if (attribute is not null)
                typeName = attribute.DisplayName;

            if (typeName.IsNotEmpty()) return typeName;

            if (type.IsGenericType)
            {
                if (withTypeName)
                {
                    var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                    typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
                }
                else
                {
                    var keyName = type.Name;
                    var index = keyName.IndexOf('`');
                    typeName = index == -1 ? keyName : keyName.Substring(0, index);
                }
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static T Bind<T>(this T model, Expression<Func<T, object>> expression, object value)
        {
            return model.Bind<T, object>(expression, value);
        }

        public static T BindId<T>(this T model, Expression<Func<T, Guid>> expression)
        {
            return model.Bind(expression, Guid.NewGuid());
        }

        private static TModel Bind<TModel, TProperty>([NotNull] this TModel model,
            [NotNull] Expression<Func<TModel, TProperty>> expression,
            object value)
        {
            if (model is null) throw new ArgumentException(null, nameof(model));

            if (!(expression.Body is MemberExpression memberExpression))
                memberExpression = ((UnaryExpression) expression.Body).Operand as MemberExpression ??
                                   throw new InvalidOperationException();

            var propertyName = memberExpression.Member.Name.ToLowerInvariant();
            var modelType = model.GetType();
            var field = modelType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith($"<{propertyName}>"));
            if (field is null) return model;

            field.SetValue(model, value);

            return model;
        }

        public static List<SelectListItem> ToSelectListItems(this Type type)
        {
            return type.GetConstants()
                .Select(x => new SelectListItem(x.Name, $"{x.GetRawConstantValue()}"))
                .ToList();
        }

        public static Dictionary<string, string> ToDictionaryFieldInfo(this Type type)
        {
            return type.GetConstants().ToDictionary(x => x.Name, x => $"{x.GetRawConstantValue()}");
        }

        public static string ToJsonFieldInfo(this Type type)
        {
            var dictionaryFieldInfo = type.ToDictionaryFieldInfo();

            var json = JsonConvert.SerializeObject(dictionaryFieldInfo, Formatting.Indented,
                new JsonSerializerSettings
                {
                    MaxDepth = 3,
                    Formatting = Formatting.None,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });
            return json;
        }

        public static IEnumerable<FieldInfo> GetConstants(this Type type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var fields = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly);

            return fields;
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

        public static IEnumerable<StackFrame> GetStackTraces(this Type type)
            => new StackTrace().GetFrames()
                .Where(x => x.GetMethod()?.DeclaringType?.Name == type.Name);


        public static string GetJsonStackTrace(this Type type)
        {
            var stacktrace = type.GetStackTraces();
            var stackFrameModels = stacktrace.Select(stackFrame => stackFrame.GetStackFrame()).ToList();

            var jsonDetails = Serializer.Serialize(stackFrameModels);

            return jsonDetails;
        }

        public static Type GetLastCalledType([NotNull] this Type currentType)
        {
            var frames = new StackTrace().GetFrames();

            var i = 0;
            while (frames[i].GetMethod()?.DeclaringType == currentType ||
                   frames[i].GetMethod()?.DeclaringType == typeof(TypeExtensions))
            {
                i++;
            }

            return new StackTrace().GetFrames()[i].GetMethod()?.DeclaringType ?? typeof(TypeExtensions);
        }

        public static T? Clone<T>(this T? source)
        {
            if (source is null) return default;
            var serialized = JsonConvert.SerializeObject(source, Serializer.DefaultJsonSettings);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static T? Construct<T>([NotNull] this Type type, Type[] paramTypes, object[] paramValues) where T : class
        {
            var ci = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null, paramTypes, null);

            if (ci is null)
            {
                ci = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null, paramTypes, null);
            }

            return ci?.Invoke(paramValues) as T;
        }

        public static string UniqueTypeName(this Type type)
        {
            if (type is null) return "";
            var name = type.Name.ToUnderscore();
            var nameSpace = type.Namespace?.ToLowerUnderscore();
            return $"{nameSpace}_{name}".ToLowerInvariant();
        }

        public static string GetRootPath(this Type type)
        {
            var location = type.Assembly.Location;
            var index = location.IndexOf("bin", StringComparison.Ordinal);
            return location.Substring(0, index);
        }

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
                    if (instance.IsNull() == false) return instance;
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
                if (instance.IsNull())
                    throw new InstanceException(type);
                return instance;
            }
            catch
            {
                try
                {
                    instance = Activator.CreateInstance(type,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args,
                        System.Threading.Thread.CurrentThread.CurrentCulture);
                    if (instance.IsNull())
                        throw new InstanceException(type);
                    return instance;
                }
                catch
                {
                    try
                    {
                        instance = Activator.CreateInstance(type, true);
                        if (instance.IsNull())
                            throw new InstanceException(type);
                        return instance;
                    }
                    catch
                    {
                        throw new InstanceException(type);
                    }
                }
            }
        }

        public static string CacheKey(this Type type, string key, char separator = ':')
            => $"{type.Name}{separator}{key}";

        public static List<PropertyInfo> GetSettingsProperties(this Type type)
        {
            if (!typeof(ISettings).IsAssignableFrom(type) || type.IsAbstract) return new List<PropertyInfo>();

            return type.GetProperties().Where(x =>
                x.CanWrite &&
                x.PropertyType.IsClass &&
                !x.PropertyType.IsAbstract &&
                typeof(IOptions).IsAssignableFrom(x.PropertyType)).ToList();
        }

        public static bool ShouldBeSingleton(this Type type)
            => typeof(IShouldBeSingleton).IsAssignableFrom(type);

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                yield return implementedInterface;
            }

            var baseType = typeInfo.BaseType;

            while (baseType != null)
            {
                var baseTypeInfo = baseType.GetTypeInfo();

                yield return baseType;

                baseType = baseTypeInfo.BaseType;
            }
        }

        public static bool IsInNamespace(this Type type, string @namespace)
        {
            var typeNamespace = type.Namespace ?? string.Empty;

            if (@namespace.Length > typeNamespace.Length)
            {
                return false;
            }

            var typeSubNamespace = typeNamespace.Substring(0, @namespace.Length);

            if (typeSubNamespace.Equals(@namespace, StringComparison.Ordinal))
            {
                if (typeNamespace.Length == @namespace.Length)
                    return true;

                return typeNamespace[@namespace.Length] == '.';
            }

            return false;
        }

        public static bool IsInExactNamespace(this Type type, string @namespace)
        {
            return string.Equals(type.Namespace, @namespace, StringComparison.Ordinal);
        }

        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return type.GetTypeInfo().IsDefined(attributeType, inherit: true);
        }

        public static bool HasAttribute<T>(this Type type, Func<T, bool> predicate) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>(inherit: true).Any(predicate);
        }

        public static bool IsAssignableTo(this Type type, Type otherType)
        {
            var typeInfo = type.GetTypeInfo();
            var otherTypeInfo = otherType.GetTypeInfo();

            if (otherTypeInfo.IsGenericTypeDefinition)
            {
                return typeInfo.IsAssignableToGenericTypeDefinition(otherTypeInfo);
            }

            return otherTypeInfo.IsAssignableFrom(typeInfo);
        }

        public static IEnumerable<Type> FindMatchingInterface(this TypeInfo typeInfo,
            Action<TypeInfo, IFluentTypeFilter>? action)
        {
            var matchingInterfaceName = $"I{typeInfo.Name}";

            var matchedInterfaces = GetImplementedInterfacesToMap(typeInfo)
                .Where(x => string.Equals(x.Name, matchingInterfaceName, StringComparison.Ordinal))
                .ToArray();

            Type? type;
            if (action is null)
            {
                type = matchedInterfaces.FirstOrDefault();
            }
            else
            {
                var filter = new FluentTypeFilter(matchedInterfaces);

                action(typeInfo, filter);

                type = filter.Types.FirstOrDefault();
            }

            if (type is null)
            {
                yield break;
            }

            yield return type;
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool HasMatchingGenericArity(this Type interfaceType, TypeInfo typeInfo)
        {
            if (typeInfo.IsGenericType)
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();

                if (interfaceTypeInfo.IsGenericType)
                {
                    var argumentCount = interfaceType.GenericTypeArguments.Length;
                    var parameterCount = typeInfo.GenericTypeParameters.Length;

                    return argumentCount == parameterCount;
                }

                return false;
            }

            return true;
        }

        public static Type GetRegistrationType(this Type interfaceType, TypeInfo typeInfo)
        {
            if (typeInfo.IsGenericTypeDefinition)
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();

                if (interfaceTypeInfo.IsGenericType)
                {
                    return interfaceType.GetGenericTypeDefinition();
                }
            }

            return interfaceType;
        }

        public static string GetTableName(this Type aggregateRootType)
        {
            var tableAttribute = aggregateRootType.GetCustomAttribute<TableAttribute>();

            return tableAttribute != null && tableAttribute.Name.IsNotEmpty()
                ? tableAttribute.Name
                : aggregateRootType.Name;
        }

        public static bool IsValidPrimaryType(this Type type)
        {
            return IsSimpleTypeCache.GetOrAdd(type, t =>
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
                IsNullableSimpleType(type));

            static bool IsNullableSimpleType(Type t)
            {
                var underlyingType = Nullable.GetUnderlyingType(t);
                return underlyingType != null && IsValidPrimaryType(underlyingType);
            }
        }


        public static string ToLowerUnderscore(this Type type, char separator = '.')
        {
            var name = type.FullName ?? type.Name;
            return name.ToLowerUnderscore(separator);
        }

        public static string ToUnderscore([NotNull] this Type type)
        {
            var name = type.FullName ?? type.Name;
            return name.ToUnderscore();
        }

        #region Helpers

        private static string PrettyPrintRecursive([NotNull] Type type, int depth)
        {
            if (depth > 3) return type.Name;

            var nameParts = type.Name.Split('`');
            if (nameParts.Length == 1) return nameParts[0];

            var genericArguments = type.GetTypeInfo().GetGenericArguments();
            return !type.IsConstructedGenericType
                ? $"{nameParts[0]}<{new string(',', genericArguments.Length - 1)}>"
                : $"{nameParts[0]}<{string.Join(",", genericArguments.Select(t => PrettyPrintRecursive(t, depth + 1)))}>";
        }

        private static bool GenericParametersMatch(IReadOnlyList<Type> parameters,
            IReadOnlyList<Type> interfaceArguments)
        {
            if (parameters.Count != interfaceArguments.Count)
            {
                return false;
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] != interfaceArguments[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAssignableToGenericTypeDefinition(this TypeInfo typeInfo, TypeInfo genericTypeInfo)
        {
            var interfaceTypes = typeInfo.ImplementedInterfaces.Select(t => t.GetTypeInfo());

            foreach (var interfaceType in interfaceTypes)
            {
                if (interfaceType.IsGenericType)
                {
                    var typeDefinitionTypeInfo = interfaceType
                        .GetGenericTypeDefinition()
                        .GetTypeInfo();

                    if (typeDefinitionTypeInfo.Equals(genericTypeInfo))
                    {
                        return true;
                    }
                }
            }

            if (typeInfo.IsGenericType)
            {
                var typeDefinitionTypeInfo = typeInfo
                    .GetGenericTypeDefinition()
                    .GetTypeInfo();

                if (typeDefinitionTypeInfo.Equals(genericTypeInfo))
                {
                    return true;
                }
            }

            var baseTypeInfo = typeInfo.BaseType?.GetTypeInfo();

            if (baseTypeInfo is null)
            {
                return false;
            }

            return baseTypeInfo.IsAssignableToGenericTypeDefinition(genericTypeInfo);
        }

        private static IEnumerable<Type> GetImplementedInterfacesToMap(TypeInfo typeInfo)
        {
            if (!typeInfo.IsGenericType)
            {
                return typeInfo.ImplementedInterfaces;
            }

            if (!typeInfo.IsGenericTypeDefinition)
            {
                return typeInfo.ImplementedInterfaces;
            }

            return FilterMatchingGenericInterfaces(typeInfo);
        }

        private static IEnumerable<Type> FilterMatchingGenericInterfaces(TypeInfo typeInfo)
        {
            var genericTypeParameters = typeInfo.GenericTypeParameters;

            foreach (var current in typeInfo.ImplementedInterfaces)
            {
                var currentTypeInfo = current.GetTypeInfo();

                if (currentTypeInfo.IsGenericType && currentTypeInfo.ContainsGenericParameters
                                                  && GenericParametersMatch(genericTypeParameters,
                                                      currentTypeInfo.GenericTypeArguments))
                {
                    yield return currentTypeInfo.GetGenericTypeDefinition();
                }
            }
        }

        #endregion
    }
}