using System;
using System.Globalization;

namespace Kharazmi.Helpers
{
    public static class DateTimeHelper
    {
        public static CultureInfo CultureInfo => System.Threading.Thread.CurrentThread.CurrentCulture;
        public static CultureInfo CurrentUICulture => System.Threading.Thread.CurrentThread.CurrentUICulture;
        public static TimeZoneInfo TimeZoneInfoUtc => TimeZoneInfo.Utc;
        public static TimeZoneInfo TimeZoneInfoLocal => TimeZoneInfo.Local;

        public static DateTime DateTimeNow =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfoLocal.Id);

        public static DateTimeOffset DateTimeOffsetNow =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, TimeZoneInfoLocal.Id);

        public static DateTime DateTimeUtcNow =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneInfoUtc.Id);

        public static DateTimeOffset DateTimeOffsetUtcNow =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, TimeZoneInfoUtc.Id);

        public static string LocalAsString =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfoLocal.Id).ToString("O");

        public static string UtcAsString =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, TimeZoneInfoUtc.Id).ToString("O");

        public static DateTime LocalFromString(string value)
        {
            DateTime.TryParse(value, out var dateTime);
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TimeZoneInfoLocal.Id);
        }

        public static DateTimeOffset UtcFromString(string value)
        {
            DateTimeOffset.TryParse(value, out var dateTime);
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TimeZoneInfoUtc.Id);
        }

        public static DateTimeOffset ToUtc(this DateTimeOffset value) =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, TimeZoneInfoUtc.Id);
    }
}