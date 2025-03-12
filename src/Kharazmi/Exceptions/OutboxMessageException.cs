using System;

namespace Kharazmi.Exceptions
{
    /// <summary>_</summary>
    public class OutboxMessageException : Exception
    {
        public OutboxMessageException(string message) : base(message)
        {
        }

        public OutboxMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static OutboxMessageException Create(string message) => new(message);

        /// <summary>/ </summary>
        public static OutboxMessageException Empty => new OutboxMessageException("");
    }
}