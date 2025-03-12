#region

using System;
using Kharazmi.Dependency;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.LoggerProviders
{
    public class NullLogger : ILogger, INullInstance
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
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