#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Helpers;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Models
{
    [Serializable]
    public class MessageModel
    {
        /// <summary> </summary>
        /// <param name="description"></param>
        /// <param name="code"></param>
        /// <param name="messageType"></param>
        [JsonConstructor]
        protected MessageModel(string? description, string? code, string? messageType)
        {
            Code = code;
            Description = description;
            MessageType = messageType;
            CreateAt = DateTimeHelper.DateTimeOffsetUtcNow.UtcDateTime.ToString("g");
            MessageId = Guid.NewGuid().ToString("N");
        }

        /// <summary> </summary>
        [JsonIgnore]
        public string CreateAt { get; }

        /// <summary> </summary>
        [JsonIgnore]
        public string MessageId { get; }

        /// <summary> </summary>
        [JsonProperty]
        public string? Code { get; }

        /// <summary> </summary>
        [JsonProperty]
        public string? Description { get; }

        /// <summary> </summary>
        [JsonIgnore]
        public string? MessageType { get; protected set; }

        /// <summary> </summary>
        public static MessageModel Empty => new("", "", "Error");

        /// <summary> </summary>
        /// <param name="description"></param>
        /// <param name="code"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static MessageModel For(string? description, string? code = "", string? messageType = "Error")
        {
            return new(description, code, messageType);
        }

        public static MessageModel From([NotNull] Result result)
        {
            return new(result.Description, result.Code, result.ResultStatus);
        }

        /// <summary> </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public MessageModel UpdateMessageType(string? type)
        {
            MessageType = type;
            return this;
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append("#ResultMessage with: ");
            builder.Append($"{{MessageId}}: {MessageId}");
            builder.Append($"{{MessageType}}: {MessageType}");
            builder.Append($"{{CreateAt}}: {CreateAt}");

            if (Code.IsNotEmpty())
                builder.Append($"{{Code}}: {Code}");
            if (Description.IsNotEmpty())
                builder.Append($"{{Description}}: {Description}");

            return builder.ToString();
        }
    }
}