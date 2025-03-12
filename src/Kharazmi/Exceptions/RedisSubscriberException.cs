using System;

namespace Kharazmi.Exceptions
{
    public class RedisSubscriberException : Exception
    {
        public RedisSubscriberException(string message) : base(message)
        {
        }

        public RedisSubscriberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static RedisSubscriberException Create(string message)
        {
            return new RedisSubscriberException(message);
        }

        /// <summary>/ </summary>
        public static RedisSubscriberException Empty => new RedisSubscriberException("");
    }
}