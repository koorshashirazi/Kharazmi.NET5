using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.RabbitMq
{
    /// <summary> </summary>
    public class MessageNamingConventions : NestedOption
    {
        public MessageNamingConventions()
        {
        }

        [Newtonsoft.Json.JsonConstructor, JsonConstructor]
        public MessageNamingConventions(string exchangeName, string queueName, string routingKey, string? typeFullName = null)
        {
            ExchangeName = exchangeName;
            QueueName = queueName;
            RoutingKey = routingKey;
            TypeFullName = typeFullName;
        }

        public string ExchangeName { get; set; }

        /// <summary> Get or set NameSpace plus type name</summary>
        [StringLength(200)]
        public string? TypeFullName { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string QueueName { get; set; }

        /// <summary> </summary>
        [StringLength(200)]
        public string RoutingKey { get; set; }

        public override void Validate()
        {
            if (TypeFullName.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MessageNamingConventions), nameof(TypeFullName)));

            if (QueueName.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MessageNamingConventions), nameof(QueueName)));

            if (RoutingKey.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MessageNamingConventions), nameof(RoutingKey)));
        }
    }
}