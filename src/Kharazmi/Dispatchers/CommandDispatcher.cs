#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Handlers;
using Kharazmi.Messages;
using Newtonsoft.Json.Linq;

#endregion

namespace Kharazmi.Dispatchers
{
    internal class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceFactory;

        public CommandDispatcher(IServiceProvider serviceFactory)
            => _serviceFactory = serviceFactory;

        public async Task<Result> SendAsync(
            object? command,
            Type commandType,
            MetadataCollection? metadata = null,
            CancellationToken token = default)
        {
            var commandObj = command switch
            {
                JObject => JObject.FromObject(command).ToObject(commandType),
                ICommand => command,
                _ => default
            };

            if (commandObj is not ICommand message)
                return Result.Fail("Invalid command object or command type, Command dispatcher can't send command");

            var commandHandlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            dynamic? commandHandler = _serviceFactory.GetService(commandHandlerType);

            if (commandHandler is null)
                return Result.Fail($"can't get any implementation of {commandHandlerType.Name}");

            var domainMetadata = DomainMetadata.Empty;
            domainMetadata.AddRange(metadata);

            dynamic result = await commandHandler.HandleAsync(message, domainMetadata, token);
            return result;
        }
    }
}