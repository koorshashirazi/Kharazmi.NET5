using System;

namespace Kharazmi.Constants
{
    public static class ExpirationConstants
    {
        public static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5);
    }
}