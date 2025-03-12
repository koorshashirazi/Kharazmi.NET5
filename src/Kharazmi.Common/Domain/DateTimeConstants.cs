using System;

namespace Kharazmi.Common.Domain
{
    internal static class DateTimeConstants
    {
        public static DateTimeOffset UtcNow =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneInfo.Utc.Id);

        public static DateTimeOffset ToUtc(this DateTime value) =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, TimeZoneInfo.Utc.Id);

        public const string MaxAllowedDayFormat =
            "The {0} value must be positive. Maximum allowed {2} days. Given: {1}";

        public const string MaxAllowedMillisecondsFormat =
            "The {0} value must be positive. Maximum allowed {2} Milliseconds. Given: {1}";

        public const string MaxAllowedHourFormat =
            "The {0} value must be positive. Maximum allowed {2} hour. Given: {1}";

        public static string MaxAllowedMilliseconds(string name, TimeSpan value) => string.Format(
            MaxAllowedMillisecondsFormat, name, value, TimeSpan.FromMilliseconds(int.MaxValue));

        public static string MaxAllowedHour(string name, TimeSpan value, TimeSpan max) =>
            string.Format(MaxAllowedHourFormat, name, value, max);

        public static string MaxAllowedDay(string name, TimeSpan value, TimeSpan max) =>
            string.Format(MaxAllowedDayFormat, name, value, max);

        // public static DateTimeOffset UtcNow => new(DateTime.UtcNow, TimeSpan.Zero);
    }
}