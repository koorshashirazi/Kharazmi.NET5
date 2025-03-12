using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class OutboxPersistenceProviders
    {
        public static IReadOnlyCollection<string> GetPersistenceProviders => new[]
        {
            HostMemory,
            Distributed,
            Redis,
            Mongo,
            Ef
        };

        public const string HostMemory = "HostMemory";
        public const string Distributed = "Distributed";
        public const string Redis = "Redis";
        public const string Mongo = "Mongo";
        public const string Ef = "Ef";
    }
}