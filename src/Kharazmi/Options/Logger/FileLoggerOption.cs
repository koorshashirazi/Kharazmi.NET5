using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Logger;

namespace Kharazmi.Options.Logger
{
    public class FileLoggerOption : NestedOption
    {
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        private Encoding? _encoding;

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        private IFormatProvider? _formatProvider;

        private object? _textFormatter;

        public FileLoggerOption()
        {
            Level = LogEventLevels.Error;
            FilePath =  $"Logging\\{Guid.NewGuid():N}.json";
            OutputTemplate = LoggerConstants.DefaultOutputTemplate;
            FileSizeLimitBytes = LoggerConstants.DefaultFileSizeLimitBytes;
            RetainedFileCountLimit = LoggerConstants.DefaultRetainedFileCountLimit;
            IsBuffered = false;
            IsShared = false;
            RollOnFileSizeLimit = true;
            RollingIntervalType = RollingIntervalTypes.Hour;
            UseJsonFormatter = true;
        }


        public string Level { get; set; }
        [Bindable(false)] public IReadOnlyCollection<string> Levels => LogEventLevels.GetLogEventLevels();
        public string? SwitchLevelTo { get; set; }
        public bool UseJsonFormatter { get; set; }
        public string FilePath { get; set; }
        public string OutputTemplate { get; set; }
        public long FileSizeLimitBytes { get; set; }
        public bool IsBuffered { get; set; }
        public bool IsShared { get; set; }
        public TimeSpan? FlushDiskInterval { get; set; }
        public bool RollOnFileSizeLimit { get; set; }
        public string RollingIntervalType { get; set; }
        [Bindable(false)] public IReadOnlyCollection<string> RollingIntervals => RollingIntervalTypes.GetTypes();
        public int RetainedFileCountLimit { get; set; }

        public Encoding? Encoding() => _encoding;

        public IFormatProvider? FormatProvider() => _formatProvider;
        public object? TextFormatter() => _textFormatter;

        public FileLoggerOption SetEncoder(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public FileLoggerOption SetTextFormat(object textFormatter)
        {
            _textFormatter = textFormatter;
            return this;
        }

        public FileLoggerOption SetFormatProvider(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
            return this;
        }

        public override void Validate()
        {
            if (Level.IsEmpty())
                Level = LogEventLevels.Error;

            if (FilePath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(FileLoggerOption), nameof(FilePath)));
        }
    }
}