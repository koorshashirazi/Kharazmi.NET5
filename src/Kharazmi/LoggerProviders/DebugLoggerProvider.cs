#region

using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.LoggerProviders
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DebugLogger(categoryName);
        }
    }
}