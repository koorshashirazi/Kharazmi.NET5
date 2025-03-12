using System;
using System.Collections.Generic;
using Kharazmi.Common.Metadata;
using Kharazmi.Constants;

namespace Kharazmi.Options.HealthCheck
{
    public class HealthCheckOption : NestedOption
    {
        public HealthCheckOption()
        {
            Tags = new HashSet<string>();
            Name = Guid.NewGuid().ToString("N");
            CheckTimeOut = TimeSpan.FromSeconds(10);
            Attempts = -1;
            Metadata = new MetadataCollection();
            Period = TimeSpan.FromSeconds(30);
        }

        public static HealthCheckOption Empty => new();

        public string Name { get; set; }
        public HashSet<string> Tags { get; set; }
        public TimeSpan CheckTimeOut { get; set; }
        public TimeSpan Period { get; set; }
        public int Attempts { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ApiKey { get; set; }
        public MetadataCollection Metadata { get; set; }


        public override void Validate()
        {
            if (CheckTimeOut <= TimeSpan.Zero && CheckTimeOut != System.Threading.Timeout.InfiniteTimeSpan)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HealthCheckOption), nameof(CheckTimeOut), CheckTimeOut));

            if (Period <= TimeSpan.Zero && Period != System.Threading.Timeout.InfiniteTimeSpan)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HealthCheckOption), nameof(Period), Period));
        }
    }
}