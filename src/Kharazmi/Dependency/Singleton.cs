using System;
using Kharazmi.Exceptions;

namespace Kharazmi.Dependency
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        protected Singleton(){}

        public static T Instance { get; } = Create();

        private static T Create() 
        {
            Type t = typeof(T);
            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic;
            var constructor = t.GetConstructor(flags, null, Type.EmptyTypes, null);
            var instance = constructor?.Invoke(null);
            return instance as T ?? throw new InstanceException(typeof(T));
        }
    }
}