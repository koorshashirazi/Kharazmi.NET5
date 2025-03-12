using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kharazmi.Constants;
using Kharazmi.ElasticSearch;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Logger;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Logger
{
    public class ElasticsearchLoggerOption : NestedOption, IHaveHealthCheckOption
    {
        private Func<string, DateTime, string>? _bufferIndexDecider;
        private Func<string, long?, string, string>? _bufferCleanPayload;
        private Action<LogEvent>? _failureCallback;
        private Func<LogEvent, string>? _pipelineNameDecider;
        private Func<LogEvent, DateTimeOffset, string>? _indexDecider;

        public ElasticsearchLoggerOption()
        {
            AutoRegisterTemplate = false;
            OverwriteTemplate = false;
            InlineFields = false;
            FormatStackTraceAsArray = false;
            BatchPostingLimit = 50;
            QueueSizeLimit = 100000;
            BufferFileCountLimit = 31;
            BufferFileSizeLimitBytes = 104857600L;
            NodeUris = "http://localhost:9200";
            IndexFormat = "logstash-{0:yyyy.MM.dd}";
            DeadLetterIndexName = "deadletter-{0:yyyy.MM.dd}";
            TypeName = "logevent";
            TemplateName = "serilog-events-template";
            BatchAction = ElasticOpTypes.Index;
            EmitEventFailure = EmitEventFailureHandlings.WriteToSelfLog;
            RegisterTemplateFailure = RegisterTemplateRecoveries.IndexAnyway;
            Level = LogEventLevels.Verbose;
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersions.ESv7;
            Period = TimeSpan.FromSeconds(2.0);
            ConnectionTimeout = TimeSpan.FromSeconds(5.0);
            TemplateCustomSettings = new();
            IndexAliases = Array.Empty<string>();
            ConnectionSettings = new ConnectionConfiguration();
        }

        public bool AutoRegisterTemplate { get; set; }
        public bool OverwriteTemplate { get; set; }
        public bool FormatStackTraceAsArray { get; set; }
        public bool DetectElasticsearchVersion { get; set; }
        public bool InlineFields { get; set; }
        public int? NumberOfShards { get; set; }
        public int? NumberOfReplicas { get; set; }
        public int BatchPostingLimit { get; set; }
        public int QueueSizeLimit { get; set; }
        public int? BufferFileCountLimit { get; set; }
        public long? SingleEventSizePostingLimit { get; set; }
        public long? BufferFileSizeLimitBytes { get; set; }
        public long? BufferRetainedInvalidPayloadsLimitBytes { get; set; }
        public string Level { get; set; }
        [Bindable(false)] public IReadOnlyCollection<string> Levels => LogEventLevels.GetLogEventLevels();
        public string? SwitchLevelTo { get; set; }
        public string NodeUris { get; set; }
        public string TemplateName { get; set; }
        public string IndexFormat { get; set; }
        public string DeadLetterIndexName { get; set; }
        public string TypeName { get; set; }
        public string AutoRegisterTemplateVersion { get; set; }
        public string RegisterTemplateFailure { get; set; }
        public string BatchAction { get; set; }
        public string EmitEventFailure { get; set; }
        public string? PipelineName { get; set; }
        public string? BufferBaseFilename { get; set; }
        public TimeSpan Period { get; set; }
        public TimeSpan ConnectionTimeout { get; set; }
        public TimeSpan? BufferLogShippingInterval { get; set; }
        public string[] IndexAliases { get; set; }
        public Dictionary<string, string> TemplateCustomSettings { get; set; }
        public ConnectionConfiguration ConnectionSettings { get; set; }
        public string? ConnectionPoolTypeAssembly { get; set; }
        public string? FormatProviderTypeAssembly { get; set; }
        public string? ConnectionTypeAssembly { get; set; }
        public string? SerializerTypeAssembly { get; set; }
        public string? CustomFormatterTypeAssembly { get; set; }
        public string? CustomDurableFormatterTypeAssembly { get; set; }
        public string? FailureSinkTypeAssembly { get; set; }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public Func<string, DateTime, string>? BufferIndexDecider() => _bufferIndexDecider;

        public ElasticsearchLoggerOption SetBufferIndexDecider(Func<string, DateTime, string> handler)
        {
            _bufferIndexDecider = handler;
            return this;
        }

        public Func<string, long?, string, string>? BufferCleanPayload() => _bufferCleanPayload;

        public ElasticsearchLoggerOption SetBufferCleanPayload(Func<string, long?, string, string> handler)
        {
            _bufferCleanPayload = handler;
            return this;
        }

        public Action<LogEvent>? FailureCallback() => _failureCallback;

        public ElasticsearchLoggerOption SetFailureCallback(Action<LogEvent> handler)
        {
            _failureCallback = handler;
            return this;
        }

        public Func<LogEvent, string>? PipelineNameDecider() => _pipelineNameDecider;

        public ElasticsearchLoggerOption SetPipelineNameDecider(Func<LogEvent, string> handler)
        {
            _pipelineNameDecider = handler;
            return this;
        }

        public Func<LogEvent, DateTimeOffset, string>? IndexDecider() => _indexDecider;

        public ElasticsearchLoggerOption SetIndexDecider(Func<LogEvent, DateTimeOffset, string> handler)
        {
            _indexDecider = handler;
            return this;
        }


        public override void Validate()
        {
            if (Level.IsEmpty())
                Level = LogEventLevels.Verbose;

            if (NodeUris.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ElasticsearchLoggerOption), nameof(NodeUris)));

            if (!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}