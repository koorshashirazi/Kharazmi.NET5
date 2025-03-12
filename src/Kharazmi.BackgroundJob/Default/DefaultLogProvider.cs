using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Kharazmi.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Hangfire.Default
{
    internal class DefaultLogProvider : global::Hangfire.Logging.ILogProvider
    {
        private readonly global::Hangfire.Logging.LogLevel _minLevel;
        private readonly ILogger<DefaultLog> _logger;

        static DefaultLogProvider()
            => MessageFormatter = DefaultMessageFormatter;

        public DefaultLogProvider([AllowNull]ILoggerFactory? factory)
        {
            _logger = factory?.CreateLogger<DefaultLog>() ?? NullLogger<DefaultLog>.Instance;
            _minLevel = global::Hangfire.Logging.LogLevel.Trace;
        }


        public global::Hangfire.Logging.ILog GetLogger(string name) => new DefaultLog(_logger, name, _minLevel);

        private static MessageFormatterDelegate MessageFormatter { get;  }

        private static string DefaultMessageFormatter(
            string loggerName,
            global::Hangfire.Logging.LogLevel level,
            object message,
            Exception? e)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture));
            stringBuilder.Append(" ");
            stringBuilder.Append(("[" + level.ToString().ToUpper() + "]").PadRight(8));
            stringBuilder.Append("(" + loggerName + ") ");
            stringBuilder.Append(message);

            if (e == null) return stringBuilder.ToString();

            stringBuilder.Append(Environment.NewLine).Append(e.GetType());
            stringBuilder.Append(Environment.NewLine).Append(e.Message);
            stringBuilder.Append(Environment.NewLine).Append(e.StackTrace);

            return stringBuilder.ToString();
        }


        private delegate string MessageFormatterDelegate(
            string loggerName,
            global::Hangfire.Logging.LogLevel level,
            object message,
            Exception? e);

        internal class DefaultLog : global::Hangfire.Logging.ILog
        {
            private static readonly object Lock = new();
            private readonly string _name;
            private readonly global::Hangfire.Logging.LogLevel _minLevel;
            private readonly ILogger<DefaultLog>? _logger;


            public DefaultLog(
                [AllowNull]ILogger<DefaultLog>? logger,
                string name,
                global::Hangfire.Logging.LogLevel minLevel)
            {
                _name = name;
                _minLevel = minLevel;
                _logger = logger;
            }

            public bool Log(global::Hangfire.Logging.LogLevel logLevel, Func<string>? messageFunc, Exception exception)
            {
                if (logLevel < _minLevel)
                    return false;
                if (messageFunc == null)
                    return true;
                Write(logLevel, messageFunc(), exception);
                return true;
            }

            private void Write(global::Hangfire.Logging.LogLevel logLevel, string message, Exception? e = null)
            {
                string str = MessageFormatter(_name, logLevel, message, e);
                
                if (str.IsEmpty()) return;
                
                lock (Lock)
                {
                    switch (logLevel)
                    {
                        case global::Hangfire.Logging.LogLevel.Trace:
                            _logger?.LogTrace(str, e);
                            break;
                        case global::Hangfire.Logging.LogLevel.Debug:
                            _logger?.LogTrace(str, e);
                            break;
                        case global::Hangfire.Logging.LogLevel.Info:
                            _logger?.LogTrace(str, e);
                            break;
                        case global::Hangfire.Logging.LogLevel.Warn:
                            _logger?.LogWarning(str, e);
                            break;
                        case global::Hangfire.Logging.LogLevel.Error:
                            _logger?.LogError(str, e);
                            break;
                        case global::Hangfire.Logging.LogLevel.Fatal:
                            _logger?.LogCritical(str, e);
                            break;
                    }
                }
            }
        }
    }
}