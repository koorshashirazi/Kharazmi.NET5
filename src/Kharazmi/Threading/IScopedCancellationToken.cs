#region

using System.Threading;

#endregion

namespace Kharazmi.Threading
{
    public interface IScopedCancellationToken
    {
        CancellationTokenSource TokenSource { get; }
        CancellationToken CancellationToken { get; }
        void CancelScope();
    }
}