#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Dispatchers
{
    public interface ICommandDispatcher : IMustBeInstance
    {
        Task<Result> SendAsync(
            object? command,
            Type commandType,
            MetadataCollection? metadata = null,
            CancellationToken token = default);
    }
}