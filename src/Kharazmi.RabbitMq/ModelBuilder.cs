using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Kharazmi.Http;
using Kharazmi.Options.RabbitMq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Kharazmi.RabbitMq
{
    /// <summary> </summary>
    internal class ModelBuilder
    {
        /// <summary></summary>
        /// <param name="option"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static RabbitMqOption CreateAsync([NotNull] RabbitMqOption option, ILogger<ModelBuilder>? logger)
        {
            try
            {
                var strategy = option.ModelBuilderStrategy;

                if (strategy == RabbitMqModelBuilderStrategies.Ignore) return option;

                var factory = new ConnectionFactory
                {
                    HostName = option.HostNames.FirstOrDefault(),
                    Port = option.Port,
                    VirtualHost = option.VirtualHost,
                };

                if (option.Username.IsNotEmpty() && option.Password.IsNotEmpty())
                {
                    factory.UserName = option.Username;
                    factory.Password = option.Password;
                }

                var exchangeName = option.ExchangeName;
                exchangeName = exchangeName.NotEmpty("ExchangeName");
                var errorExchange = CustomNamingConventions.ErrorExchangeNaming(option);

                using var connection = factory.CreateConnection();

                using var httpClientFactory = HttpClientFactory.Instance;
                var client = httpClientFactory.GetOrCreate(option.ApiBaseUrl);
                client.SetBasicAuthentication(option.Username, option.Password);

                var sourceBindings = client.GetFromJsonAsync<SourceBindings[]>("api/bindings").GetAwaiter().GetResult();
                var sourceBinding =
                    sourceBindings?.Where(x =>
                            string.Equals(x.Vhost, option.VirtualHost, StringComparison.OrdinalIgnoreCase) &&
                            (string.Equals(x.Source, exchangeName, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(x.Source, errorExchange, StringComparison.OrdinalIgnoreCase)))
                        .ToList() ??
                    Enumerable.Empty<SourceBindings>().ToList();

                var conventions = option.MessageNamingConventions;

                switch (strategy)
                {
                    case RabbitMqModelBuilderStrategies.Ignore:
                        return option;
                    case RabbitMqModelBuilderStrategies.DeleteIfExist:

                        if (ExistExchange(option, exchangeName, client))
                            DeleteExchange(connection, exchangeName);
                        if (ExistExchange(option, errorExchange, client))
                            DeleteExchange(connection, errorExchange);

                        foreach (var binding in sourceBinding)
                        {
                            if (ExistQueue(option, binding.Destination, client))
                                DeleteQueue(connection, binding.Destination);
                        }

                        return option;

                    case RabbitMqModelBuilderStrategies.CreateIfNotExist:
                        if (!ExistExchange(option, exchangeName, client))
                            CreateExchange(option, connection, exchangeName);

                        if (!ExistExchange(option, errorExchange, client))
                            CreateExchange(option, connection, errorExchange);

                        foreach (var binding in sourceBinding)
                        {
                            var convention =
                                conventions.FirstOrDefault(x =>
                                    string.Equals(x.QueueName, binding.Destination, StringComparison.OrdinalIgnoreCase)) ??
                                new MessageNamingConventions(exchangeName, binding.Destination, binding.RoutingKey);

                           if(convention.RoutingKey.IsEmpty()) continue;

                            if (!ExistQueue(option, convention.QueueName, client))
                                CreateQueue(option, connection, convention, exchangeName);
                        }

                        foreach (var convention in conventions)
                        {
                            if (!ExistQueue(option, convention.QueueName, client))
                                CreateQueue(option, connection, convention, exchangeName);
                        }

                        return option;

                    case RabbitMqModelBuilderStrategies.DeleteAndCreateOnce:

                        DeleteAndCreate(option, connection, exchangeName, errorExchange, sourceBinding, conventions,
                            client);

                        option.ModelBuilderStrategy = RabbitMqModelBuilderStrategies.Ignore;

                        return option;

                    case RabbitMqModelBuilderStrategies.AlwaysDeleteAndCreate:

                        DeleteAndCreate(option, connection, exchangeName, errorExchange, sourceBinding, conventions,
                            client);

                        return option;
                }

                switch (strategy)
                {
                    case RabbitMqModelBuilderStrategies.IgnoreExchange:
                        break;
                    case RabbitMqModelBuilderStrategies.DeleteExchangeIfExist:
                        if (ExistExchange(option, exchangeName, client))
                            DeleteExchange(connection, exchangeName);

                        if (ExistExchange(option, errorExchange, client))
                            DeleteExchange(connection, errorExchange);

                        return option;

                    case RabbitMqModelBuilderStrategies.CreateExchangeIfNotExist:
                        if (!ExistExchange(option, exchangeName, client))
                            CreateExchange(option, connection, exchangeName);

                        if (!ExistExchange(option, errorExchange, client))
                            CreateExchange(option, connection, errorExchange);

                        return option;

                    case RabbitMqModelBuilderStrategies.AlwaysDeleteAndCreateExchange:
                        DeleteExchange(connection, exchangeName);
                        DeleteExchange(connection, errorExchange);
                        CreateExchange(option, connection, exchangeName);
                        CreateExchange(option, connection, errorExchange);
                        return option;

                    case RabbitMqModelBuilderStrategies.DeleteAndCreateExchangeOnce:

                        DeleteExchange(connection, exchangeName);
                        DeleteExchange(connection, errorExchange);
                        CreateExchange(option, connection, exchangeName);
                        CreateExchange(option, connection, errorExchange);
                        option.ModelBuilderStrategy = RabbitMqModelBuilderStrategies.IgnoreExchange;
                        return option;
                }

                switch (strategy)
                {
                    case RabbitMqModelBuilderStrategies.IgnoreQueue:
                        return option;

                    case RabbitMqModelBuilderStrategies.DeleteQueueIfExist:
                        foreach (var binding in sourceBinding)
                        {
                            if (ExistQueue(option, binding.Destination, client))
                                DeleteQueue(connection, binding.Destination);
                        }

                        return option;

                    case RabbitMqModelBuilderStrategies.CreateQueueIfNotExist:
                        foreach (var binding in sourceBinding)
                        {
                            var convention =
                                conventions.FirstOrDefault(x =>
                                    string.Equals(x.QueueName, binding.Destination,
                                        StringComparison.OrdinalIgnoreCase)) ??
                                new MessageNamingConventions(exchangeName, binding.Destination,
                                    binding.RoutingKey);

                            if (convention.RoutingKey.IsEmpty()) continue;
                            if (!ExistQueue(option, convention.QueueName, client))
                                CreateQueue(option, connection, convention, exchangeName);
                        }

                        foreach (var convention in conventions)
                        {
                            if (!ExistQueue(option, convention.QueueName, client))
                                CreateQueue(option, connection, convention, exchangeName);
                        }

                        return option;

                    case RabbitMqModelBuilderStrategies.DeleteAndCreateQueueOnce:
                        foreach (var binding in sourceBinding)
                            DeleteQueue(connection, binding.Destination);

                        foreach (var binding in sourceBinding)
                        {
                            var convention =
                                conventions.FirstOrDefault(x =>
                                    string.Equals(x.QueueName, binding.Destination,
                                        StringComparison.OrdinalIgnoreCase)) ??
                                new MessageNamingConventions(exchangeName, binding.Destination, binding.RoutingKey);
                            if (convention.RoutingKey.IsEmpty()) continue;
                            CreateQueue(option, connection, convention, exchangeName);
                        }

                        foreach (var convention in conventions)
                        {
                            if (!ExistQueue(option, convention.QueueName, client))
                                CreateQueue(option, connection, convention, exchangeName);
                        }

                        option.ModelBuilderStrategy = RabbitMqModelBuilderStrategies.IgnoreQueue;
                        return option;

                    case RabbitMqModelBuilderStrategies.AlwaysDeleteAndCreateQueue:
                        foreach (var binding in sourceBinding)
                            DeleteQueue(connection, binding.Destination);

                        foreach (var binding in sourceBinding)
                        {
                            var convention =
                                conventions.FirstOrDefault(x =>
                                    string.Equals(x.QueueName, binding.Destination,
                                        StringComparison.OrdinalIgnoreCase)) ??
                                new MessageNamingConventions(exchangeName, binding.Destination, binding.RoutingKey);

                            if (convention.RoutingKey.IsEmpty()) continue;

                            CreateQueue(option, connection, convention, exchangeName);
                        }

                        foreach (var convention in conventions)
                        {
                            if (!ExistQueue(option, convention.QueueName, client))
                                CreateQueue(option, connection, convention, exchangeName);
                        }

                        return option;
                }

                return option;
            }
            catch (Exception e)
            {
                logger?.LogError("{Message}", e.Message);
                if (option.ThrowOnModelBuild)
                    throw new RabbitMqModelBuilderException(e.Message);
                return option;
            }
        }

        private static void DeleteExchange(IConnection connection, string exchangeName)
        {
            using var channel = connection.CreateModel();
            channel.ExchangeDelete(exchangeName, false);
        }


        private static void DeleteQueue(IConnection connection, string queueName)
        {
            using var channel = connection.CreateModel();
            channel.QueueDelete(queueName, false, false);
        }

        private static void DeleteAndCreate(RabbitMqOption option, IConnection connection, string exchangeName,
            string errorExchange, List<SourceBindings> sourceBinding, List<MessageNamingConventions> conventions,
            HttpClient client)
        {
            var channel = connection.CreateModel();
            channel.ExchangeDelete(exchangeName, false);
            channel.ExchangeDelete(errorExchange, false);

            foreach (var binding in sourceBinding)
                channel.QueueDelete(binding.Destination, false, false);

            CreateExchange(option, connection, exchangeName);
            CreateExchange(option, connection, errorExchange);

            foreach (var binding in sourceBinding)
            {
                var convention =
                    conventions.FirstOrDefault(x =>
                        string.Equals(x.QueueName, binding.Destination, StringComparison.OrdinalIgnoreCase)) ??
                    new MessageNamingConventions(exchangeName, binding.Destination, binding.RoutingKey);

                if (convention.RoutingKey.IsEmpty()) continue;
                CreateQueue(option, connection, convention, exchangeName);
            }


            foreach (var convention in conventions)
            {
                if (!ExistQueue(option, convention.QueueName, client))
                    CreateQueue(option, connection, convention, exchangeName);
            }
        }


        private static void CreateQueue(RabbitMqOption option, IConnection connection,
            MessageNamingConventions namingConventions,
            string sExchangeName)
        {
            using var channel = connection.CreateModel();
            var queueOptions = option.Queue;
            var qName = namingConventions.QueueName;

            if (qName.IsEmpty())
                throw new NullReferenceException("QueueName");

            if (queueOptions is null)
                throw new NullReferenceException(nameof(GeneralQueueConfiguration));

            var ok = channel.QueueDeclare(qName, queueOptions.Durable, queueOptions.Exclusive,
                queueOptions.AutoDelete);

            if (ok.QueueName.IsNotEmpty())
                channel.QueueBind(qName, sExchangeName, namingConventions.RoutingKey);
        }

        private static void CreateExchange(RabbitMqOption option, IConnection connection, string sExchangeName)
        {
            using var channel = connection.CreateModel();
            var exchange = option.Exchange;

            if (exchange is null)
                throw new NullReferenceException(nameof(GeneralExchangeConfiguration));

            var valuesType = exchange.Type;

            channel.ExchangeDeclare(sExchangeName, valuesType.ToLower(), exchange.Durable,
                exchange.AutoDelete);
        }

        private static bool ExistExchange(RabbitMqOption option, string exchangeName, HttpClient client)
        {
            var sourceBindings = client.GetFromJsonAsync<SourceBindings[]>("api/bindings").GetAwaiter().GetResult();
            var sourceBinding =
                sourceBindings?.FirstOrDefault(x =>
                    x.Vhost == option.VirtualHost &&
                    string.Equals(x.Source, exchangeName, StringComparison.CurrentCultureIgnoreCase));
            return sourceBinding.HasValue && sourceBinding.Value.Source.IsNotEmpty();
        }

        private static bool ExistQueue(RabbitMqOption option, string queueName, HttpClient client)
        {
            var sourceBindings = client.GetFromJsonAsync<SourceBindings[]>("api/bindings").GetAwaiter().GetResult();
            var sourceBinding =
                sourceBindings?.FirstOrDefault(x =>
                    x.Vhost == option.VirtualHost &&
                    string.Equals(x.Destination, queueName, StringComparison.CurrentCultureIgnoreCase));
            return sourceBinding.HasValue && sourceBinding.Value.Destination.IsNotEmpty();
        }
    }
}