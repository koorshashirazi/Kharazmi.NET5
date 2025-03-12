using System;
using System.Collections.Generic;

namespace Kharazmi.Logger
{
    public class LogReported
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; }
        public string MessageTemplate { get; set; }
        public string RawReport { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}