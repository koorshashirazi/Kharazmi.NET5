using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using RawRabbit;
using RawRabbit.Pipe;

namespace Kharazmi.RabbitMq.Default
{
    internal class NullBusClient : IBusClient, INullInstance
    {
        public Task<IPipeContext> InvokeAsync(Action<IPipeBuilder> pipeCfg, Action<IPipeContext> contextCfg = null,
            CancellationToken token = default)
            => Task.FromResult<IPipeContext>(new PipeContext());
    }
}