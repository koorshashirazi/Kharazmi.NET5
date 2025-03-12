#region

using System;

#endregion

namespace Kharazmi.Exceptions
{
    public class InvalidTypeException : FrameworkException
    {
        public InvalidTypeException()
        {
        }

        public InvalidTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidTypeException(string parameterName) : base($"The parameter '{parameterName}' is not valid.")
        {
        }

        public InvalidTypeException(Type type, string parameterName) : base(
            $"The parameter '{type.FullName}.{parameterName}' ins not valid.")
        {
        }
    }
}