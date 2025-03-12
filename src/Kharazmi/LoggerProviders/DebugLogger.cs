#region

using System;
using System.Diagnostics;
using Kharazmi.Helpers;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.LoggerProviders
{
    public interface IDebugLogger : ILogger
    {
    }

    public class DebugLogger : IDebugLogger
    {
        private readonly string _categoryName;

        public DebugLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);

                message = $"DateTime: {DateTimeHelper.DateTimeOffsetUtcNow.UtcDateTime:g}\nState: {message}\n";

                if (exception != null)
                    message = $"{message}{Environment.NewLine}{exception}";

                Debugger.Log((int) logLevel, _categoryName, message);
                return;
            }

            Debugger.Log((int) logLevel, Enum.GetName(typeof(LogLevel), logLevel), "");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}