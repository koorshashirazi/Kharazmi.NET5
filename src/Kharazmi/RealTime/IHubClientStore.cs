using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.RealTime
{
    public interface IHubClientStore : IMustBeInstance
    {
        Result<bool> Exist(ClientHubInfo clientHubInfo);
        Task<Result<bool>> ExistAsync(ClientHubInfo clientHubInfo, CancellationToken token = default);
        Maybe<ClientHubInfo> Find(ClientHubInfo clientHubInfo);
        Task<Maybe<ClientHubInfo>> FindAsync(ClientHubInfo clientHubInfo, CancellationToken token = default);
        Maybe<ClientHubInfo> FindBy(Func<ClientHubInfo, bool> predicate);
        Task<Maybe<ClientHubInfo>> FindByAsync(Func<ClientHubInfo, bool> predicate, CancellationToken token = default);
        IEnumerable<ClientHubInfo> GetBy(Func<ClientHubInfo, bool> predicate);

        Task<IEnumerable<ClientHubInfo>> GetByAsync(Func<ClientHubInfo, bool> predicate,
            CancellationToken token = default);

        IEnumerable<ClientHubInfo> GetAll();
        Task<IEnumerable<ClientHubInfo>> GetAllAsync();
        Result AddOrUpdateClient(ClientHubInfo[] clientHubInfos, TimeSpan? expiresIn = null);

        Task<Result> AddOrUpdateClientAsync(ClientHubInfo[] clientHubInfos, TimeSpan? expiresIn = null,
            CancellationToken token = default);

        Result RemoveClient(ClientHubInfo[] clientHubInfos);
        Task<Result> RemoveClientAsync(ClientHubInfo[] clientHubInfos, CancellationToken token = default);
    }
}