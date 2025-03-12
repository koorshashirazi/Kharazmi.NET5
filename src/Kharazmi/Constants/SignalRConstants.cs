using System;
using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class SignalRConstants
    {
        public static HashSet<TimeSpan>? DefaultRetryDelaysInMilliseconds = new(new[]
        {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            default  
        });
    }
}