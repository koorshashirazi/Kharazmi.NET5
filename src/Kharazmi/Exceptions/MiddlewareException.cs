using System;
using Kharazmi.Functional;

namespace Kharazmi.Exceptions
{
    public class MiddlewareException : FrameworkException
    {
        public MiddlewareException()
        {
        }

        public MiddlewareException(string message) : base(message)
        {
        }

        public MiddlewareException(string message, Exception exceptions) : base(message, exceptions)
        {
        }

        public static MiddlewareException For(Result result) =>
            new() {Code = result.Code, Description = result.Description};
    }
}