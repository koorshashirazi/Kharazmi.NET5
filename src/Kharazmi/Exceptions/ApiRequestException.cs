using System;
using Kharazmi.Functional;

namespace Kharazmi.Exceptions
{
    public class ApiRequestException : FrameworkException
    {
        public ApiRequestException()
        {
        }

        public ApiRequestException(string message) : base(message)
        {
        }

        public ApiRequestException(string message, Exception exceptions) : base(message, exceptions)
        {
        }

        public static ApiRequestException For(Result result) =>
            new() {Code = result.Code, Description = result.Description};
    }
}