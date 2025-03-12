using System.Collections.Generic;
using Kharazmi.Options;

namespace Kharazmi.HealthChecks
{
    public interface IHealthChecker
    {
       IEnumerable<string> GetUnHealthOptions<TOption>() where TOption : class, IOptions;
    }
}