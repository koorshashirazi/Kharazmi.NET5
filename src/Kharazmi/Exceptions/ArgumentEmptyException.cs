#region

using System;

#endregion

namespace Kharazmi.Exceptions
{
    public class ArgumentEmptyException : FrameworkException
    {
        public ArgumentEmptyException()
        {
        }

        public ArgumentEmptyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ArgumentEmptyException(string parameterName) : base(
            $"The parameter '{parameterName}' cannot be null or an empty string.")
        {
        }

        public ArgumentEmptyException(Type type, string parameterName) : base(
            $"The parameter '{type.FullName}.{parameterName}' cannot be null or an empty string.")
        {
        }
    }
}