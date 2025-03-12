#region

using System;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Kharazmi.Exceptions;

#endregion

namespace Kharazmi.RabbitMq
{
    /// <summary>_</summary>
    public interface IBusHandler: IMustBeInstance
    {
        /// <summary>_</summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        IBusHandler Handle(Func<Task> handle);

        /// <summary>_</summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        IBusHandler OnSuccess(Func<Task> onSuccess);

        /// <summary>_</summary>
        /// <param name="onError"></param>
        /// <param name="rethrow"></param>
        /// <returns></returns>
        IBusHandler OnError(Func<Exception, Task> onError, bool rethrow = false);

        /// <summary>_</summary>
        /// <param name="onCustomError"></param>
        /// <param name="rethrow"></param>
        /// <returns></returns>
        IBusHandler OnCustomError(Func<MessageBusException, Task> onCustomError, bool rethrow = false);

        /// <summary>_</summary>
        /// <param name="always"></param>
        /// <returns></returns>
        IBusHandler Always(Func<Task> always);

        /// <summary>_</summary>
        /// <returns></returns>
        Task ExecuteAsync();
    }
}