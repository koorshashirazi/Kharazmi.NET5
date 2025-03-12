using System;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Extensions
{
    public static class ValueObjectExtensions
    {
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
        
    }
}