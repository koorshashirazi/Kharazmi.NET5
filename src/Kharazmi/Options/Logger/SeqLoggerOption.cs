using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Logger;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Logger
{
    public class SeqLoggerOption : NestedOption, IHaveHealthCheckOption
    {
        public SeqLoggerOption()
        {
            Level = LogEventLevels.Verbose;
            ServerUrl = "http://localhost:11433";
            BatchPostingLimit = 1000;
            EventBodyLimitBytes = 262144;
            QueueSizeLimit = 100000;
            Compact = false;
            Period = TimeSpan.FromSeconds(2.0);
        }

        public string Level { get; set; }

        [Bindable(false)] public IReadOnlyCollection<string> Levels => LogEventLevels.GetLogEventLevels();
        public string? SwitchLevelTo { get; set; }
        public string ServerUrl { get; set; }
        public int BatchPostingLimit { get; set; }
        public TimeSpan Period { get; set; }
        public string? ApiKey { get; set; }
        public string? BufferBaseFilename { get; set; }
        public long? BufferSizeLimitBytes { get; set; }
        public long EventBodyLimitBytes { get; set; }
        public long? RetainedInvalidPayloadsLimitBytes { get; set; }
        public int QueueSizeLimit { get; set; }
        public bool Compact { get; set; }
        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (Level.IsEmpty())
                Level = LogEventLevels.Verbose;

            if (ServerUrl.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(SeqLoggerOption), nameof(ServerUrl)));
            if (UseHealthCheck == false) return;
            HealthCheckOption ??= HealthCheckOption.Empty;
            HealthCheckOption.Validate();
        }
    }
}