using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kharazmi.Models
{
    [Serializable]
    public class ExecuteResult
    {
        [JsonProperty] public string? Code { get; set; }
        [JsonProperty] public string? Reason { get; set; }
        [JsonProperty] public int Status { get; set; }

        [JsonProperty] public IEnumerable<MessageModel>? Messages { get; set; }

        [JsonProperty] public IEnumerable<ValidationFailure>? ValidationMessages { get; set; }
    }
}