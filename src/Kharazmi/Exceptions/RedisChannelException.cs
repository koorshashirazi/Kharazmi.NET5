using System;

namespace Kharazmi.Exceptions
{
    public class RedisChannelException : Exception
    {
        public RedisChannelException(string message) : base(message)
        {
        }

        public RedisChannelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static RedisChannelException Create(string message)
        {
            return new RedisChannelException(message);
        }

        /// <summary>/ </summary>
        public static RedisChannelException Empty => new RedisChannelException("");
    }
}