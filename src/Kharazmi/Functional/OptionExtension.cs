using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Functional
{
    public static class OptionExtension
    {
        public static Option<T> When<T>(this T obj, bool condition) =>
            condition ? (Option<T>) new Some<T>(obj) : None.Value;

        public static Option<T> When<T>(this T obj, Func<T, bool> predicate) =>
            obj.When(predicate(obj));

        public static Option<T> NoneIfNull<T>(this T obj) =>
            obj.When(!object.ReferenceEquals(obj, null));

        public static Option<T> FirstOrNone<T>(this IEnumerable<T> sequence) =>
            sequence.Select(x => (Option<T>) new Some<T>(x))
                .DefaultIfEmpty(None.Value)
                .First();

        public static Option<T> FirstOrNone<T>(
            this IEnumerable<T> sequence, Func<T, bool> predicate) =>
            sequence.Where(predicate).FirstOrNone();

        public static IEnumerable<TResult> SelectOptional<T, TResult>(
            this IEnumerable<T> sequence, Func<T, Option<TResult>> map) =>
            sequence.Select(map)
                .OfType<Some<TResult>>()
                .Select(some => some.Value);

        public static Option<TValue> TryGetValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.TryGetValue(key, out var value)
                ? (Option<TValue>) new Some<TValue>(value)
                : None.Value;
    }
}