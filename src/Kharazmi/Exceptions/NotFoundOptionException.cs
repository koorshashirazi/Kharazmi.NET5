using System;

namespace Kharazmi.Exceptions
{
    public class NotFoundOptionException : Exception
    {
        public NotFoundOptionException(string optionType, string? optionKey) : base(
            $"Can't find option with type {optionType}  option key {optionKey}")
        {
        }

        public NotFoundOptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}