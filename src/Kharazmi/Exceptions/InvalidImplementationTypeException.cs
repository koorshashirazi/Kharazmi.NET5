using System;

namespace Kharazmi.Exceptions
{
    public class InvalidImplementationTypeException : FrameworkException
    {
        public InvalidImplementationTypeException()
        {
        }

        public InvalidImplementationTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidImplementationTypeException(string parameterName) : base($"The Implementation of type is not assignable from '{parameterName}'.")
        {
        }

        public InvalidImplementationTypeException(Type type, Type parameterName) : base(
            $"The Implementation of {type.Name} is not assignable from '{parameterName.Name}'.")
        {
        }
    }
}