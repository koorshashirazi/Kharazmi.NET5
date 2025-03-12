using System.Collections.Generic;

namespace Kharazmi.Logger
{
    public readonly struct LogEventLevels
    {
        public static IReadOnlyCollection<string> GetLogEventLevels() => new[]
        {
            Verbose,
            Debug,
            Information,
            Warning,
            Error,
            Fatal
        };

        public const string Verbose = "Verbose";
        public const string Debug = "Debug";
        public const string Information = "Information";
        public const string Warning = "Warning";
        public const string Error = "Error";
        public const string Fatal = "Fatal";
    }
}