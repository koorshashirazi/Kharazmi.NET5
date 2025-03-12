using System;

namespace Kharazmi.Options.Hangfire
{
    public class HangfireInMemoryStorageOption : NestedOption
    {
        public HangfireInMemoryStorageOption()
        {
            JobExpirationCheckInterval = TimeSpan.FromHours(1.0);
            CountersAggregateInterval = TimeSpan.FromMinutes(5.0);
        }

        public TimeSpan JobExpirationCheckInterval { get; set; }

        public TimeSpan CountersAggregateInterval { get; set; }
        public override void Validate()
        {
            
        }
    }
}