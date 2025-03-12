using Newtonsoft.Json;

namespace Kharazmi.Common.Events
{
    /// <summary>_</summary>
    public sealed class HubEvent
    {
        /// <summary>_</summary>
        /// <param name="message"></param>
        /// <param name="messageName"></param>
        [JsonConstructor]
        public HubEvent(string message, string messageName)
        {
            Message = message;
            MessageName = messageName;
        }

        /// <summary>_</summary>
        public string Message { get; }
        /// <summary>_</summary>
        public string MessageName { get; set; }

        /// <summary>_</summary>
        public string? UserId { get; set; }

        /// <summary>_</summary>
        public string? UserName { get; set; }
    }
}