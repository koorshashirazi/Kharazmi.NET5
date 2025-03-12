using System.Threading;

namespace Kharazmi.Threading
{
    public class GlobalScopedCancellationToken : IGlobalCancellationToken
    {
        public CancellationTokenSource TokenSource { get; }

        public GlobalScopedCancellationToken()
            => TokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => TokenSource.Token;

        public void CancelScope()
            => TokenSource.Cancel();
    }
}