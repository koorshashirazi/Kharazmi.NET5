using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Logger;

namespace Kharazmi.Options.Logger
{
    public class ConsoleLoggerOption : NestedOption
    {
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        private IFormatProvider? _formatProvider;

        public ConsoleLoggerOption()
        {
            Level = LogEventLevels.Information;
            OutputTemplate = LoggerConstants.DefaultOutputTemplate;
        }

        public string Level { get; set; }
        [Bindable(false)] public IReadOnlyCollection<string> Levels => LogEventLevels.GetLogEventLevels();
        public string? SwitchLevelTo { get; set; }
        public string? StandardErrorFromLevel { get; set; }
        public string OutputTemplate { get; set; }
        public IFormatProvider? FormatProvider() => _formatProvider;

        public ConsoleLoggerOption SetFormatProvider(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
            return this;
        }

        public override void Validate()
        {
            if (Level.IsEmpty())
                Level = LogEventLevels.Information;
        }
    }
}