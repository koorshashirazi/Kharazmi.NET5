#region

using System;

#endregion

namespace Kharazmi.Retries
{
    public class RetryDelay
    {
        private static readonly Random Random = new();

        private RetryDelay(TimeSpan min, TimeSpan max)
        {
            if (min.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(min), "Minimum cannot be negative");
            if (max.Ticks < 0) throw new ArgumentOutOfRangeException(nameof(max), "Maximum cannot be negative");

            Min = min;
            Max = max;
        }

        public TimeSpan Max { get; }

        public TimeSpan Min { get; }


        public static RetryDelay Between(TimeSpan min, TimeSpan max)
            => new(min, max);

        public TimeSpan PickDelay()
        {
            if (Max.TotalMilliseconds - Min.TotalMilliseconds <= 0)
                return TimeSpan.Zero;

            return Min.Add(
                TimeSpan.FromMilliseconds((Max.TotalMilliseconds - Min.TotalMilliseconds) * Random.NextDouble()));
        }
    }
}