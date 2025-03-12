using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kharazmi.Extensions;
using Kharazmi.Guard;

namespace Kharazmi.Runtime
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFluentTypeFilter
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        string? ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object? obj);

        IFluentTypeFilter AssignableTo<T>();

        IFluentTypeFilter AssignableTo(Type type);

        IFluentTypeFilter AssignableToAny(params Type[] types);
        IFluentTypeFilter AssignableToAny(IEnumerable<Type> types);

        IFluentTypeFilter WithAttribute<T>() where T : Attribute;
        IFluentTypeFilter WithAttribute(Type attributeType);
        IFluentTypeFilter WithAttribute<T>(Func<T, bool> predicate) where T : Attribute;
        IFluentTypeFilter WithoutAttribute<T>() where T : Attribute;

        IFluentTypeFilter WithoutAttribute(Type attributeType);
        IFluentTypeFilter WithoutAttribute<T>(Func<T, bool> predicate) where T : Attribute;
        IFluentTypeFilter InNamespaceOf<T>();

        IFluentTypeFilter InNamespaceOf(params Type[] types);

        IFluentTypeFilter InNamespaces(params string[] namespaces);

        IFluentTypeFilter InExactNamespaceOf<T>();

        IFluentTypeFilter InExactNamespaceOf(params Type[] types);

        IFluentTypeFilter InExactNamespaces(params string[] namespaces);

        IFluentTypeFilter InNamespaces(IEnumerable<string> namespaces);

        IFluentTypeFilter NotInNamespaceOf<T>();

        IFluentTypeFilter NotInNamespaceOf(params Type[] types);

        IFluentTypeFilter NotInNamespaces(params string[] namespaces);

        IFluentTypeFilter NotInNamespaces(IEnumerable<string> namespaces);

        IFluentTypeFilter Where(Func<Type, bool> predicate);
    }

    internal class FluentTypeFilter : IFluentTypeFilter
    {
        public FluentTypeFilter(IEnumerable<Type> types)
        {
            Types = types;
        }

        internal IEnumerable<Type> Types { get; private set; }

        public IFluentTypeFilter AssignableTo<T>()
        {
            return AssignableTo(typeof(T));
        }

        public IFluentTypeFilter AssignableTo(Type type)
        {
            type.NotNull(nameof(type));

            return AssignableToAny(type);
        }

        public IFluentTypeFilter AssignableToAny(params Type[] types)
        {
            types.NotNull(nameof(types));

            return AssignableToAny(types.AsEnumerable());
        }

        public IFluentTypeFilter AssignableToAny(IEnumerable<Type> types)
        {
            types = types.NotNull(nameof(types));

            return Where(t => types.Any(t.IsAssignableTo));
        }

        public IFluentTypeFilter WithAttribute<T>() where T : Attribute
        {
            return WithAttribute(typeof(T));
        }

        public IFluentTypeFilter WithAttribute(Type attributeType)
        {
            attributeType.NotNull(nameof(attributeType));

            return Where(t => t.HasAttribute(attributeType));
        }

        public IFluentTypeFilter WithAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            predicate.NotNull(nameof(predicate));

            return Where(t => t.HasAttribute(predicate));
        }

        public IFluentTypeFilter WithoutAttribute<T>() where T : Attribute
        {
            return WithoutAttribute(typeof(T));
        }

        public IFluentTypeFilter WithoutAttribute(Type attributeType)
        {
            attributeType.NotNull(nameof(attributeType));

            return Where(t => !t.HasAttribute(attributeType));
        }

        public IFluentTypeFilter WithoutAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            predicate.NotNull(nameof(predicate));

            return Where(t => !t.HasAttribute(predicate));
        }

        public IFluentTypeFilter InNamespaceOf<T>()
        {
            return InNamespaceOf(typeof(T));
        }

        public IFluentTypeFilter InNamespaceOf(params Type[] types)
        {
            types.NotNull(nameof(types));

            return InNamespaces(types.Select(t => t.Namespace ?? string.Empty));
        }

        public IFluentTypeFilter InNamespaces(params string[] namespaces)
        {
            namespaces.NotNull(nameof(namespaces));

            return InNamespaces(namespaces.AsEnumerable());
        }

        public IFluentTypeFilter InExactNamespaceOf<T>()
        {
            return InExactNamespaceOf(typeof(T));
        }

        public IFluentTypeFilter InExactNamespaceOf(params Type[] types)
        {
            types.NotNull(nameof(types));
            return Where(t => types.Any(x => t.IsInExactNamespace(x.Namespace ?? string.Empty)));
        }

        public IFluentTypeFilter InExactNamespaces(params string[] namespaces)
        {
            namespaces.NotNull(nameof(namespaces));

            return Where(t => namespaces.Any(t.IsInExactNamespace));
        }

        public IFluentTypeFilter InNamespaces(IEnumerable<string> namespaces)
        {
            namespaces = namespaces.NotNull(nameof(namespaces));

            return Where(t => namespaces.Any(t.IsInNamespace));
        }

        public IFluentTypeFilter NotInNamespaceOf<T>()
        {
            return NotInNamespaceOf(typeof(T));
        }

        public IFluentTypeFilter NotInNamespaceOf(params Type[] types)
        {
            types.NotNull(nameof(types));

            return NotInNamespaces(types.Select(t => t.Namespace ?? string.Empty));
        }

        public IFluentTypeFilter NotInNamespaces(params string[] namespaces)
        {
            namespaces.NotNull(nameof(namespaces));

            return NotInNamespaces(namespaces.AsEnumerable());
        }

        public IFluentTypeFilter NotInNamespaces(IEnumerable<string> namespaces)
        {
            namespaces = namespaces.NotNull(nameof(namespaces));

            return Where(t => namespaces.All(ns => !t.IsInNamespace(ns)));
        }

        public IFluentTypeFilter Where(Func<Type, bool> predicate)
        {
            predicate.NotNull(nameof(predicate));

            Types = Types.Where(predicate);
            return this;
        }
    }
}