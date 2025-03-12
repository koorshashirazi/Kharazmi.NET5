using System;
using Kharazmi.Functional;

namespace Kharazmi.Exceptions
{
    public class ServiceResolverException : FrameworkException
    {
        public ServiceResolverException()
            : this("Can't resolve any service implementation of requested ServiceType")
        {
        }

        public ServiceResolverException(Type serviceType, Type implementationType) : this(
                $"Can't resolve any service implementation with type {implementationType.Name} of {serviceType.Name}")
        {
        }

        public ServiceResolverException(string message) : base(message)
        {
        }

        public ServiceResolverException(string message, Exception exceptions) : base(message, exceptions)
        {
        }

        public static ServiceResolverException For(Result result) =>
            new() {Code = result.Code, Description = result.Description};
    }
}