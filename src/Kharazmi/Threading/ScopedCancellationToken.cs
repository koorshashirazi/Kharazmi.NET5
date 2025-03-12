#region

using System.Threading;

#endregion

namespace Kharazmi.Threading
{
    public class ScopedCancellationToken : IScopedCancellationToken
    {
        public CancellationTokenSource TokenSource { get; }

        public ScopedCancellationToken()
            => TokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => TokenSource.Token;

        public void CancelScope()
            => TokenSource.Cancel();
    }
}