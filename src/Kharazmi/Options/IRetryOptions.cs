using System.Collections.Generic;

namespace Kharazmi.Options
{
    public interface IRetryOptions
    {
        int Attempt { get; set; }
        int MinDelay { get; set; }
        int MaxDelay { get; set; }
        HashSet<string> RetryOnExceptionTypes { get; set; }
    }
}