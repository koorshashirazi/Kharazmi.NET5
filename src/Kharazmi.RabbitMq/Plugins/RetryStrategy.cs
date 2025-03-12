#region

using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using RawRabbit.Common;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;

#endregion

namespace Kharazmi.RabbitMq.Plugins
{
    internal class RetryStrategy : StagedMiddleware
    {
        public override string StageMarker { get; } = global::RawRabbit.Pipe.StageMarker.MessageDeserialized;


        public RetryStrategy()
        {
            
        }
        public override async Task InvokeAsync(IPipeContext context,
            CancellationToken token = new())
        {
            var retry = context.GetRetryInformation();
            if (context.GetMessageContext() is DomainMetadata message) message.SetRetry(retry.NumberOfRetries);

            await Next.InvokeAsync(context, token);
        }
    }
}