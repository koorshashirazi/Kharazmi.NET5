using System;
using Kharazmi.Functional;

namespace Kharazmi.Exceptions
{
    public class ServiceCollectionException : FrameworkException
    {
        public ServiceCollectionException()
        {
        }

        public ServiceCollectionException(string message) : base(message)
        {
        }

        public ServiceCollectionException(string message, Exception exceptions) : base(message, exceptions)
        {
        }

        public static ServiceCollectionException For(Result result) =>
            new() {Code = result.Code, Description = result.Description};
    }
}