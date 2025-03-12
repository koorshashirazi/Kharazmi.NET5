using System;

namespace Kharazmi.Exceptions
{
    public class RabbitMqModelBuilderException : Exception
    {
        public RabbitMqModelBuilderException(string? message) : base(message)
        {
        }

        public RabbitMqModelBuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static RabbitMqModelBuilderException Create(string message)
        {
            return new(message);
        }

        /// <summary>/ </summary>
        public static RabbitMqModelBuilderException Empty => new("");
    }
}