using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class OutboxProcessTypes
    {
        public static IReadOnlyCollection<string> GetProcessTypes() => new[]
        {
            Sequential,
            Parallel
        };

        public const string Sequential = "Sequential";
        public const string Parallel = "Parallel";
    }
}