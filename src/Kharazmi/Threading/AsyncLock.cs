#region

using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Kharazmi.Threading
{
    public sealed class AsyncLock
    {
        private readonly AsyncLockReleaser _releaser;

        public AsyncLock()
        {
            _releaser = new AsyncLockReleaser(new SemaphoreSlim(1, 1));
        }

        public IDisposable Lock()
        {
            _releaser.Lock();
            return _releaser;
        }

        public async Task<IDisposable> LockAsync(CancellationToken token = default)
        {
            await _releaser.LockAsync(token);
            return _releaser;
        }

        public async Task<IDisposable> LockAsync(TimeSpan timeout, CancellationToken token = default)
        {
            await _releaser.LockAsync(timeout, token);
            return _releaser;
        }

        private class AsyncLockReleaser : IDisposable
        {
            private readonly SemaphoreSlim _asyncLock;

            public AsyncLockReleaser(SemaphoreSlim asyncLock)
                => _asyncLock = asyncLock;

            public void Lock() => _asyncLock.Wait();

            public Task LockAsync(CancellationToken token = default)
                => LockAsync(Timeout.InfiniteTimeSpan, token);

            public Task LockAsync(TimeSpan timeout, CancellationToken token = default)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;

                // ReSharper disable once MethodSupportsCancellation
                var task = _asyncLock.WaitAsync(timeout);

                if (task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

                if (!token.CanBeCanceled)
                    return task.ContinueWith(
                        (_, s) => s as IDisposable,
                        this,
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);

                var completionSource = new TaskCompletionSource<IDisposable>();
                var registration = token.Register(() =>
                {
                    if (completionSource.TrySetCanceled())
                        task.ContinueWith(
                            (_, s) => (s as SemaphoreSlim)?.Release(),
                            _asyncLock,
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default);
                });

                task.ContinueWith(_ =>
                    {
                        if (completionSource.TrySetResult(this)) registration.Dispose();
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);

                return completionSource.Task;
            }

            public void Dispose()
                => _asyncLock.Release();
        }
    }
}