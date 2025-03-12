#region

using System;

#endregion

namespace Kharazmi.Exceptions
{
    /// <summary>_</summary>
    public class MessageBusException : Exception
    {
        public MessageBusException(string message) : base(message)
        {
        }

        public MessageBusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static MessageBusException Create(string message)
        {
            return new MessageBusException(message);
        }

        /// <summary>/ </summary>
        public static MessageBusException Empty => new MessageBusException("");
    }
}