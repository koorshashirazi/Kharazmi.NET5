using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.RealTime
{
    public sealed class NullHubClientStore : IHubClientStore, INullInstance
    {
        public Result<bool> Exist(ClientHubInfo clientHubInfo)
            => Result.FailAs<bool>("SignalR is disabled");

        public Task<Result<bool>> ExistAsync(ClientHubInfo clientHubInfo, CancellationToken token = default)
            => Task.FromResult(Result.FailAs<bool>("SignalR is disabled"));

        public Maybe<ClientHubInfo> Find(ClientHubInfo clientHubInfo)
            => Maybe<ClientHubInfo>.None;

        public Task<Maybe<ClientHubInfo>> FindAsync(ClientHubInfo clientHubInfo, CancellationToken token = default)
            => Task.FromResult(Maybe<ClientHubInfo>.None);

        public Maybe<ClientHubInfo> FindBy(Func<ClientHubInfo, bool> predicate)
            => Maybe<ClientHubInfo>.None;

        public Task<Maybe<ClientHubInfo>> FindByAsync(Func<ClientHubInfo, bool> predicate,
            CancellationToken token = default)
            => Task.FromResult(Maybe<ClientHubInfo>.None);

        public IEnumerable<ClientHubInfo> GetBy(Func<ClientHubInfo, bool> predicate)
            => Enumerable.Empty<ClientHubInfo>();

        public Task<IEnumerable<ClientHubInfo>> GetByAsync(Func<ClientHubInfo, bool> predicate,
            CancellationToken token = default) => Task.FromResult(Enumerable.Empty<ClientHubInfo>());


        public IEnumerable<ClientHubInfo> GetAll()
            => Enumerable.Empty<ClientHubInfo>();

        public Task<IEnumerable<ClientHubInfo>> GetAllAsync()
            => Task.FromResult(Enumerable.Empty<ClientHubInfo>());

        public Result AddOrUpdateClient(ClientHubInfo[] clientHubInfos, TimeSpan? expiresIn = null)
            => Result.Fail("SignalR is disabled");

        public Task<Result> AddOrUpdateClientAsync(ClientHubInfo[] clientHubInfos, TimeSpan? expiresIn = null,
            CancellationToken token = default) => Task.FromResult(Result.Fail("SignalR is disabled"));

        public Result RemoveClient(ClientHubInfo[] clientHubInfos)
            => Result.Fail("SignalR is disabled");

        public Task<Result> RemoveClientAsync(ClientHubInfo[] clientHubInfos, CancellationToken token = default)
            => Task.FromResult(Result.Fail("SignalR is disabled"));
    }
}