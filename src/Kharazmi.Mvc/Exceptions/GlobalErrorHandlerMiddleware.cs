#region

using System;
using System.Net;
using System.Threading.Tasks;
using Kharazmi.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Mvc.Exceptions
{
    internal class GlobalErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlerMiddleware> _logger;

        public GlobalErrorHandlerMiddleware(RequestDelegate next,
            ILogger<GlobalErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                await HandleErrorAsync(context, exception);
            }
        }

        private static Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            var errorCode = "error";
            var statusCode = HttpStatusCode.BadRequest;
            switch (exception)
            {
                case FrameworkException e:
                    errorCode = $"{e.HResult}";
                    break;
            }

            var response = new {code = errorCode, message = exception.Message};
            var payload = JsonConvert.SerializeObject(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) statusCode;

            return context.Response.WriteAsync(payload);
        }
    }
}