using System;
using Kharazmi.Common.Json;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

namespace Kharazmi.Background
{
    public interface IJobId : IIdentity<string>
    {
    }

    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class JobId : Identity<JobId, string>, IJobId
    {
        public JobId(string value) : base(value)
        {
        }

        public static JobId New => new(Guid.NewGuid().ToString("N"));
    }
}