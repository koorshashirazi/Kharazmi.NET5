using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kharazmi.Json
{
    internal class ConstructorResolver : DefaultContractResolver
    {
        public string ConstructorAttributeName { get; set; } = "JsonConstructorAttribute";
        public bool IgnoreAttributeConstructor { get; set; } = false;
        public bool IgnoreSinglePrivateConstructor { get; set; } = false;
        public bool IgnoreMostSpecificConstructor { get; set; } = false;

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            try
            {
                if (objectType.IsPrimitive || objectType.IsEnum) return contract;

                var overrideConstructor =
                    (IgnoreAttributeConstructor ? null : GetAttributeConstructor(objectType))
                    ?? (IgnoreSinglePrivateConstructor ? null : GetSinglePrivateConstructor(objectType))
                    ?? (IgnoreMostSpecificConstructor ? null : GetMostSpecificConstructor(objectType));

                if (overrideConstructor != null)
                {
                    SetOverrideCreator(contract, overrideConstructor);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new JsonException(e.Message);
            }

            return contract;
        }

        private void SetOverrideCreator(JsonObjectContract contract, ConstructorInfo constructor)
        {
            contract.OverrideCreator = CreateParameterizedConstructor(constructor);
            contract.CreatorParameters.Clear();
            foreach (var parameter in base.CreateConstructorParameters(constructor, contract.Properties))
            {
                contract.CreatorParameters.Add(parameter);
            }
        }

        private ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
        {
            var c = method as ConstructorInfo;
            if (c != null)
                return a => c.Invoke(a);
            return a => method.Invoke(null, a);
        }

        protected virtual ConstructorInfo? GetAttributeConstructor(Type objectType)
        {
            var constructors = objectType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(c => c.GetCustomAttributes()
                    .Any(a => a.GetType().Name == ConstructorAttributeName))
                .ToList();

            if (constructors.Count == 1) return constructors[0];
            if (constructors.Count > 1)
                throw new JsonException($"Multiple constructors with a {ConstructorAttributeName}.");

            return null;
        }

        protected virtual ConstructorInfo? GetSinglePrivateConstructor(Type objectType)
        {
            var constructors = objectType
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);


            return constructors.Length == 1 ? constructors[0].IsPrivate ? constructors[0] : null : null;
        }

        protected virtual ConstructorInfo? GetMostSpecificConstructor(Type objectType)
        {
            var constructors = objectType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.IsPrivate || x.IsPublic)
                .OrderBy(e => e.GetParameters().Length);

            var mostSpecific = constructors.LastOrDefault();
            return mostSpecific;
        }
    }
}