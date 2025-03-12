using System.Collections.Generic;

namespace Kharazmi.Options.RabbitMq
{
    public readonly struct RabbitMqModelBuilderStrategies
    {
        
        public static IReadOnlyCollection<string> GetStrategies()
        {
            return new[]
            {
                Ignore,
                DeleteIfExist,
                CreateIfNotExist,
                DeleteAndCreateOnce,
                AlwaysDeleteAndCreate,
                IgnoreExchange,
                DeleteExchangeIfExist,
                CreateExchangeIfNotExist,
                DeleteAndCreateExchangeOnce,
                AlwaysDeleteAndCreateExchange,
                IgnoreQueue,
                DeleteQueueIfExist,
                CreateQueueIfNotExist,
                DeleteAndCreateQueueOnce,
                AlwaysDeleteAndCreateQueue
            };
        } 
        public const string Ignore = "Ignore";
        public const string DeleteIfExist = "DeleteIfExist";
        public const string CreateIfNotExist = "CreateIfNotExist";
        public const string DeleteAndCreateOnce = "DeleteAndCreateOnce";
        public const string AlwaysDeleteAndCreate = "AlwaysDeleteAndCreate";
        public const string IgnoreExchange = "IgnoreExchange";
        public const string DeleteExchangeIfExist = "AlwaysDeleteExchange";
        public const string CreateExchangeIfNotExist = "CreateExchangeIfNotExist";
        public const string DeleteAndCreateExchangeOnce = "DeleteAndCreateExchangeOnce";
        public const string AlwaysDeleteAndCreateExchange = "AlwaysDeleteAndCreateExchnage";
        public const string IgnoreQueue = "IgnoreQueue";
        public const string DeleteQueueIfExist = "DeleteQueueIfExist";
        public const string CreateQueueIfNotExist = "CreateQueueIfNotExist";
        public const string DeleteAndCreateQueueOnce = "DeleteAndCreateQueueOnce";
        public const string AlwaysDeleteAndCreateQueue = "AlwaysDeleteAndCreateQueue";
    }
}