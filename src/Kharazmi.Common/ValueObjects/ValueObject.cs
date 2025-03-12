#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Kharazmi.Common.ValueObjects
{
    /// <summary> </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>_</summary>
        protected ValueObject()
        {
        }


        /// <summary>_</summary>
        protected Member[] Members => GetMembers().ToArray();

        /// <summary>_</summary>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ValueObject) obj);
        }

        /// <summary></summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ValueObject? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType() && Members.All(m =>
                {
                    var otherValue = m.GetValue(other);
                    var thisValue = m.GetValue(this);

                    return m.IsNonStringEnumerable
                        ? EqualityValues(otherValue).SequenceEqual(EqualityValues(thisValue))
                        : otherValue?.Equals(thisValue) ?? thisValue == null;
                }
            );
        }

        /// <summary> </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return CombineHashCodes(Members.Select(m => m.IsNonStringEnumerable
                ? CombineHashCodes(EqualityValues(m.GetValue(this)))
                : m.GetValue(this)));
        }

        /// <summary></summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ValueObject? left, ValueObject? right)
            => Equals(left, right);

        /// <summary></summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ValueObject? left, ValueObject? right)
            => !Equals(left, right);


        /// <summary>_</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual IEnumerable<object> EqualityValues(object? obj)
        {
            var enumerator = (obj as IEnumerable)?.GetEnumerator();
            while (enumerator != null && enumerator.MoveNext())
                yield return enumerator.Current ?? Enumerable.Empty<object>();
        }

        private static int CombineHashCodes(IEnumerable<object?> objs)
        {
            unchecked
            {
                return objs.Aggregate(17, (current, obj) => current * 59 + (obj?.GetHashCode() ?? 0));
            }
        }

        private IEnumerable<Member> GetMembers()
        {
            var t = GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            while (t != typeof(object))
            {
                if (t == null) continue;

                foreach (var p in t.GetProperties(flags)) yield return new Member(p);
                foreach (var f in t.GetFields(flags)) yield return new Member(f);

                t = t.BaseType;
            }
        }

        /// <summary> </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Members.Length == 1)
            {
                var m = Members[0];
                var value = m.GetValue(this);

                return m.IsNonStringEnumerable
                    ? $"{string.Join("|", EqualityValues(value))}"
                    : value?.ToString() ?? GetType().Name;
            }

            var values = Members.Select(
                m =>
                {
                    var value = m.GetValue(this);

                    return m.IsNonStringEnumerable
                        ? $"{m.Name}:{string.Join("|", EqualityValues(value))}"
                        : m.Type != typeof(string)
                            ? $"{m.Name}:{value}"
                            : value == null
                                ? $"{m.Name}:null"
                                : $"{m.Name}:\"{value}\"";
                }
            );
            return $"{GetType().Name}[{string.Join("|", values)}]";
        }

        protected readonly struct Member
        {
            public readonly string Name;
            public readonly Func<object, object?> GetValue;
            public readonly bool IsNonStringEnumerable;
            public readonly Type Type;

            public Member(MemberInfo info)
            {
                switch (info)
                {
                    case FieldInfo field:
                        Name = field.Name;
                        GetValue = obj => field.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(field.FieldType) &&
                                                field.FieldType != typeof(string);
                        Type = field.FieldType;
                        break;
                    case PropertyInfo prop:
                        Name = prop.Name;
                        GetValue = obj => prop.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                                                prop.PropertyType != typeof(string);
                        Type = prop.PropertyType;
                        break;
                    default:
                        throw new ArgumentException("Member is not a field or property?!?!", info.Name);
                }
            }
        }
    }

    /// <summary> </summary>
    public abstract class ValueObject<T> : IEquatable<ValueObject<T>>
        where T : ValueObject<T>
    {
        /// <summary>_</summary>
        protected ValueObject()
        {
        }


        /// <summary>_</summary>
        protected Member[] Members => GetMembers().ToArray();

        /// <summary>_</summary>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ValueObject<T>) obj);
        }

        /// <summary></summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ValueObject<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType() && Members.All(m =>
                {
                    var otherValue = m.GetValue(other);
                    var thisValue = m.GetValue(this);

                    return m.IsNonStringEnumerable
                        ? EqualityValues(otherValue).SequenceEqual(EqualityValues(thisValue))
                        : otherValue?.Equals(thisValue) ?? thisValue == null;
                }
            );
        }

        /// <summary> </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return CombineHashCodes(Members.Select(m => m.IsNonStringEnumerable
                ? CombineHashCodes(EqualityValues(m.GetValue(this)))
                : m.GetValue(this)));
        }

        /// <summary></summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ValueObject<T>? left, ValueObject<T>? right)
            => Equals(left, right);

        /// <summary></summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ValueObject<T>? left, ValueObject<T>? right)
            => !Equals(left, right);


        /// <summary>_</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual IEnumerable<object> EqualityValues(object? obj)
        {
            var enumerator = (obj as IEnumerable)?.GetEnumerator();
            while (enumerator != null && enumerator.MoveNext())
                yield return enumerator.Current ?? Enumerable.Empty<object>();
        }

        private static int CombineHashCodes(IEnumerable<object?> objs)
        {
            unchecked
            {
                return objs.Aggregate(17, (current, obj) => current * 59 + (obj?.GetHashCode() ?? 0));
            }
        }

        private IEnumerable<Member> GetMembers()
        {
            var t = GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            while (t != typeof(object))
            {
                if (t == null) continue;

                foreach (var p in t.GetProperties(flags)) yield return new Member(p);
                foreach (var f in t.GetFields(flags)) yield return new Member(f);

                t = t.BaseType;
            }
        }

        /// <summary> </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Members.Length == 1)
            {
                var m = Members[0];
                var value = m.GetValue(this);

                return m.IsNonStringEnumerable
                    ? $"{string.Join("|", EqualityValues(value))}"
                    : value?.ToString() ?? GetType().Name;
            }

            var values = Members.Select(
                m =>
                {
                    var value = m.GetValue(this);

                    return m.IsNonStringEnumerable
                        ? $"{m.Name}:{string.Join("|", EqualityValues(value))}"
                        : m.Type != typeof(string)
                            ? $"{m.Name}:{value}"
                            : value == null
                                ? $"{m.Name}:null"
                                : $"{m.Name}:\"{value}\"";
                }
            );
            return $"{GetType().Name}[{string.Join("|", values)}]";
        }

        protected readonly struct Member
        {
            public readonly string Name;
            public readonly Func<object, object?> GetValue;
            public readonly bool IsNonStringEnumerable;
            public readonly Type Type;

            public Member(MemberInfo info)
            {
                switch (info)
                {
                    case FieldInfo field:
                        Name = field.Name;
                        GetValue = obj => field.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(field.FieldType) &&
                                                field.FieldType != typeof(string);
                        Type = field.FieldType;
                        break;
                    case PropertyInfo prop:
                        Name = prop.Name;
                        GetValue = obj => prop.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                                                prop.PropertyType != typeof(string);
                        Type = prop.PropertyType;
                        break;
                    default:
                        throw new ArgumentException("Member is not a field or property?!?!", info.Name);
                }
            }
        }
    }
}