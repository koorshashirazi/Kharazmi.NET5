using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class DatabaseMigrationStrategy
    {
        public static IReadOnlyCollection<string> GetDatabaseMigrationStrategy => new[]
        {
            Ignore,
            CreateDropOnce,
            AlwaysCreateDrop,
            DropIfExist,
            CreateIfNotExist
        };

        public const string AlwaysCreateDrop = "AlwaysCreateDrop";
        public const string CreateDropOnce = "CreateDropOnce";
        public const string CreateIfNotExist = "CreateIfNotExist";
        public const string DropIfExist = "DropIfExist";
        public const string Ignore = "Ignore";

        public const string AutomaticDetectionAssembly = "*";
    }
}