using System;

namespace Kharazmi.Exceptions
{
    public class InstanceException : FrameworkException
    {
        public InstanceException()
        {
        }

        public InstanceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InstanceException(Type type) : base(
            $"Can't create a instance of type {type}. The type {type} have a public or private none parameter constructor")
        {
        }
    }
}