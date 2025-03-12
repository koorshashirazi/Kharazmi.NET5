using System.Threading;

namespace Kharazmi.Threading
{
    public interface IGlobalCancellationToken
    {
        CancellationTokenSource TokenSource { get; }
        CancellationToken CancellationToken { get; }
        void CancelScope();
    }
}