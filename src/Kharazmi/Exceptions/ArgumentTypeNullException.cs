#region

using System;
using Kharazmi.Common.Extensions;

#endregion

namespace Kharazmi.Exceptions
{
    public class ArgumentTypeNullException : FrameworkException
    {
        public ArgumentTypeNullException()
        {
        }

        public ArgumentTypeNullException(string? message, Exception? innerException) : base(message?.WithNullPrefix(),
            innerException)
        {
        }

        public ArgumentTypeNullException(string? parameterName) : base($"The parameter '{parameterName}' cannot be null."
            .WithNullPrefix())
        {
        }

        public ArgumentTypeNullException(Type type, string parameterName) : base(
            $"The parameter '{type.FullName}.{parameterName}' cannot be null.".WithNullPrefix())
        {
        }
    }
}