using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kharazmi.Common.Domain;
using Kharazmi.Common.ValueObjects;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Kharazmi.Localization
{
    public static class MongoMapper
    {
        private static readonly ConventionPack Cp;

        static MongoMapper()
        {
            UnRegisters();

            Cp = new ConventionPack();
            AddConvention("IgnoreValueObject", new IgnoreExtraElementsConvention(true),
                t => typeof(ValueObject).IsAssignableFrom(t) || typeof(ValueObject).IsAssignableFrom(t));
        }

        public static void Initial()
        {
        }

        public static void AddConvention(string name, IConvention convention, Func<Type, bool> filter)
        {
            var exist = Cp.Conventions.FirstOrDefault(x => x.Name == convention.Name);
            if (exist is not null) return;
            Cp.Add(convention);
            ConventionRegistry.Register(name, Cp, filter);
        }

        public static void TryRegister<T>(Action<BsonClassMap<T>> mapper)
        {
            RegisterAggregateRoot<string>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                BsonClassMap.RegisterClassMap(mapper);
        }

        public static void TryRegister<T, TKey>(Action<BsonClassMap<T>> mapper)
            where TKey : IEquatable<TKey>, IComparable
        {
            RegisterAggregateRoot<TKey>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                BsonClassMap.RegisterClassMap(mapper);
        }

        public static void UnRegister<T>()
            => GetRegisteredClassMapper().Remove(typeof(T));

        public static Dictionary<Type, BsonClassMap> GetRegisteredClassMapper()
        {
            var cm = BsonClassMap.GetRegisteredClassMaps().FirstOrDefault();
            if (cm is null) return new Dictionary<Type, BsonClassMap>();
            var filed = typeof(BsonClassMap).GetField("__classMaps", BindingFlags.Static | BindingFlags.NonPublic);
            var classMaps = filed?.GetValue(cm) as Dictionary<Type, BsonClassMap>;
            return classMaps ?? new Dictionary<Type, BsonClassMap>();
        }

        public static void UnRegisters() => GetRegisteredClassMapper().Clear();

       


        private static void RegisterAggregateRoot<TKey>()
            where TKey : IEquatable<TKey>, IComparable
        {
            var aggregateRootType = typeof(AggregateRoot<TKey>);
            if (BsonClassMap.IsClassMapRegistered(aggregateRootType)) return;

            BsonClassMap.RegisterClassMap<AggregateRoot<TKey>>(cm =>
            {
                cm.SetIsRootClass(true);
                cm.UnmapProperty(x => x.UncommittedEvents);
                cm.MapIdMember(p => p.Id).SetIsRequired(true);
            });

            TryRegister<AggregateRootCache<TKey>>(cm =>
            {
                cm.SetIsRootClass(true);
                cm.UnmapProperty(x => x.AbsoluteExpire);
                cm.UnmapProperty(x => x.CacheType);
                cm.MapProperty(x => x.CacheKey);
            });
        }

       
    }
}