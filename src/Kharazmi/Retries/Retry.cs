#region

using System;
using System.Collections.Generic;
using Kharazmi.Common.ValueObjects;

#endregion

namespace Kharazmi.Retries
{
    public class Retry : ValueObject
    {
        private Retry(bool shouldBeRetried, TimeSpan retryAfter)
        {
            if (retryAfter != TimeSpan.Zero && retryAfter != retryAfter.Duration())
                throw new ArgumentOutOfRangeException(nameof(retryAfter));

            if (!shouldBeRetried && retryAfter != TimeSpan.Zero)
                throw new ArgumentException("Invalid combination. Should not be retried and retry after set");

            ShouldBeRetried = shouldBeRetried;
            RetryAfter = retryAfter;
        }

        public bool ShouldBeRetried { get; }

        public TimeSpan RetryAfter { get; }

        protected override IEnumerable<object> EqualityValues(object? obj)
        {
            yield return ShouldBeRetried;
            yield return RetryAfter;
        }

        public static Retry When(Func<bool> condition, TimeSpan? timeSpan = null) =>
            new(condition(), timeSpan ?? TimeSpan.Zero);

        public static Retry Yes => new(true, TimeSpan.Zero);

        public static Retry YesAfter(TimeSpan retryAfter)
            => new(true, retryAfter);

        public static Retry No => new(false, TimeSpan.Zero);
    }
}