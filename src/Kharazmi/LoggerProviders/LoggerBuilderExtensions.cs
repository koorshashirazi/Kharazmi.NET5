#region

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.LoggerProviders
{
    public static class LoggerBuilderExtensions
    {
        public static ILoggingBuilder AddDebugLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, DebugLoggerProvider>();
            return builder;
        }
    }
}