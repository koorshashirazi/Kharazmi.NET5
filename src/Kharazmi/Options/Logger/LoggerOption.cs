using System;
using System.Collections.Generic;
using Kharazmi.HealthChecks;
using Kharazmi.Logger;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Logger
{
    public class LoggerOption : ConfigurePluginOption, IHaveHealthCheckOption, IHaveNestedHealthCheck
    {
        public LoggerOption()
        {
            MinimumLevel = LogEventLevels.Verbose;
        }

        public bool UseCorrelationDomainMetadata { get; set; }
        public string MinimumLevel { get; set; }
        public string? SwitchLevelTo { get; set; }
        public IDictionary<string, string>? MinimumLevelOverrides { get; set; }
        public IEnumerable<string>? ExcludePaths { get; set; }
        public IEnumerable<string>? ExcludeProperties { get; set; }
        public IDictionary<string, object>? Tags { get; set; }
        public bool LogApplication { get; set; }
        public bool UseConsole { get; set; }
        public ConsoleLoggerOption? ConsoleLoggerOption { get; set; }
        public bool UseFile { get; set; }
        public FileLoggerOption? FileLoggerOption { get; set; }
        public bool UseSeq { get; set; }
        public SeqLoggerOption? SeqLoggerOption { get; set; }
        public bool UseElastic { get; set; }
        public ElasticsearchLoggerOption? ElasticsearchLoggerOption { get; set; }

        public override void Validate()
        {
            if (Enable == false) return;

            MinimumLevelOverrides ??= new Dictionary<string, string>
            {
                {"Default", "Verbose"}
            };
            ExcludePaths ??= Array.Empty<string>();
            ExcludeProperties ??= Array.Empty<string>();
            Tags ??= new Dictionary<string, object>();

            if (UseConsole)
            {
                ConsoleLoggerOption ??= new ConsoleLoggerOption();
                ConsoleLoggerOption.Validate();
            }

            if (UseFile)
            {
                FileLoggerOption ??= new FileLoggerOption();
                FileLoggerOption.Validate();
            }

            if (UseSeq)
            {
                SeqLoggerOption ??= new SeqLoggerOption();
                SeqLoggerOption.Validate();
            }

            if (UseElastic)
            {
                ElasticsearchLoggerOption ??= new ElasticsearchLoggerOption();
                ElasticsearchLoggerOption.Validate();
            }

            if (UseHealthCheck == false) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public IEnumerable<INestedOption> GetNestedOption()
        {
            yield return ElasticsearchLoggerOption ?? new ElasticsearchLoggerOption();
            yield return SeqLoggerOption ?? new SeqLoggerOption();
        }
    }
}