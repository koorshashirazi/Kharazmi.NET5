using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kharazmi.Threading
{
    /// <summary>
    /// A LockProvider based upon the SemaphoreSlim class 
    /// to selectively lock objects, resources or statement blocks 
    /// according to given unique IDs in a sync or async way.
    /// </summary>
    public class AsyncLockCollection<T> : IDisposable where T : IEquatable<T>
    {
        private static readonly ConcurrentDictionary<T, SemaphoreSlim> LockDictionary = new();

        public AsyncLockCollection()
        {
        }

        public static void WaitFor([NotNull] T lockKey) => new AsyncLockCollection<T>().Wait(lockKey);
        public static void WaitAsyncFor([NotNull] T lockKey) => new AsyncLockCollection<T>().WaitAsync(lockKey);
        public static void ReleaseFor([NotNull] T lockKey) => new AsyncLockCollection<T>().Release(lockKey);


        public IReadOnlyCollection<SemaphoreSlim> GetLock => LockDictionary.Select(x => x.Value).ToList();

        /// <summary>
        /// Blocks the current thread (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="lockKey">the unique ID to perform the lock</param>
        public void Wait([NotNull] T lockKey)
            => AsyncHelper.RunSync(() => WaitAsync(lockKey));

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="lockKey">the unique ID to perform the lock</param>
        public Task WaitAsync([NotNull] T lockKey)
        {
            return LockDictionary.GetOrAdd(lockKey, new SemaphoreSlim(1, 1)).WaitAsync();
        }

        /// <summary>
        /// Releases the lock (according to the given ID)
        /// </summary>
        /// <param name="lockKey">the unique ID to unlock</param>
        public void Release([NotNull] T lockKey)
        {
            if (LockDictionary.TryGetValue(lockKey, out var semaphore))
                semaphore.Release();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            foreach (var slim in LockDictionary)
                slim.Value.Release();

            LockDictionary.Clear();
        }

        ~AsyncLockCollection()
        {
            ReleaseUnmanagedResources();
        }
    }
}