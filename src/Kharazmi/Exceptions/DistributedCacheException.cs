using System;
using Kharazmi.Functional;

namespace Kharazmi.Exceptions
{
    public class DistributedCacheException : FrameworkException
    {
        public DistributedCacheException(Result result)
        {
            Code = result.Code;
            Description = result.Description;
        }

        public DistributedCacheException(string message) : base(message)
        {
        }

        public DistributedCacheException(string message, Exception exceptions) : base(message, exceptions)
        {
        }
    }
}