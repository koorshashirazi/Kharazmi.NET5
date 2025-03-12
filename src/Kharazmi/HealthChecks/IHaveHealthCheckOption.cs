using System.Collections.Generic;
using Kharazmi.Options;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.HealthChecks
{
    public interface IHaveHealthCheckOption
    {
        public bool UseHealthCheck { get; set; }
        HealthCheckOption? HealthCheckOption { get; set; }
    }

    public interface IHaveNestedHealthCheck
    {
        IEnumerable<INestedOption?> GetNestedOption();
    }
}