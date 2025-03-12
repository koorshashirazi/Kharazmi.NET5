using System;
using System.Globalization;

namespace Kharazmi.Constants
{
    public static class DateTimeConstants
    {

        public static CultureInfo CultureInfo => System.Threading.Thread.CurrentThread.CurrentCulture;
        public static CultureInfo CurrentUICulture => System.Threading.Thread.CurrentThread.CurrentUICulture;
        public static TimeZoneInfo TimeZoneInfoUtc => TimeZoneInfo.Utc;
        public static TimeZoneInfo TimeZoneInfoLocal => TimeZoneInfo.Local;
        public static DateTimeOffset UtcNow =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneInfo.Utc.Id);

        public static DateTimeOffset ToUtc(this DateTime value) =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, TimeZoneInfo.Utc.Id);

      

     

        // public static DateTimeOffset UtcNow => new(DateTime.UtcNow, Delay.Zero);
    }
}