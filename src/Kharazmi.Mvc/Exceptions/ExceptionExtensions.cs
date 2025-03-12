#region

using Microsoft.AspNetCore.Builder;

#endregion

namespace Kharazmi.Mvc.Exceptions
{
    public static class ExceptionExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandlerMiddleware>();
        }
    }
}