using System;

namespace Kharazmi.Exceptions
{
    public class RabbitMqSubscriberException : Exception
    {
        public RabbitMqSubscriberException(string? message) : base(message)
        {
        }

        public RabbitMqSubscriberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static RabbitMqSubscriberException Create(string message)
        {
            return new RabbitMqSubscriberException(message);
        }

        /// <summary>/ </summary>
        public static RabbitMqSubscriberException Empty => new RabbitMqSubscriberException("");
    }
}