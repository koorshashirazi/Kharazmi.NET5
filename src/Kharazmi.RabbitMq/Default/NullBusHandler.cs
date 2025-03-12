using System;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Kharazmi.Exceptions;

namespace Kharazmi.RabbitMq.Default
{
    internal class NullBusHandler : IBusHandler, INullInstance
    {
        public IBusHandler Handle(Func<Task> handle)
            => this;

        public IBusHandler OnSuccess(Func<Task> onSuccess)
            => this;

        public IBusHandler OnError(Func<Exception, Task> onError, bool rethrow = false)
            => this;

        public IBusHandler OnCustomError(Func<MessageBusException, Task> onCustomError, bool rethrow = false)
            => this;

        public IBusHandler Always(Func<Task> always)
            => this;

        public Task ExecuteAsync()
            => Task.CompletedTask;
    }
}