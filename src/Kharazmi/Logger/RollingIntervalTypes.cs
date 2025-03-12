using System.Collections.Generic;

namespace Kharazmi.Logger
{
    public readonly struct RollingIntervalTypes
    {
        public static IReadOnlyCollection<string> GetTypes() => new[]
        {
            Infinite,
            Year,
            Month,
            Day,
            Hour,
            Minute
        };

        public const string Infinite = "Infinite";
        public const string Year = "Year";
        public const string Month = "Month";
        public const string Day = "Day";
        public const string Hour = "Hour";
        public const string Minute = "Minute";
    }
}