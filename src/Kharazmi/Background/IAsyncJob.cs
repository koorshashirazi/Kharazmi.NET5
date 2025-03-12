#region

using System;
using System.Threading.Tasks;

#endregion

namespace Kharazmi.Background
{
    /// <summary>
    /// To create an async job
    /// </summary>
    public interface IAsyncJob
    {
        /// <summary>
        /// To execute a job
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task ExecuteAsync(IServiceProvider provider);
    }
}