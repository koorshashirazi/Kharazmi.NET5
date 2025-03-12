using System;

namespace Kharazmi.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToStringFormat(this TimeSpan value)
            => string.Format(
                $"{value.Days:D2} days, {value.Hours:D2} hours, {value.Minutes:D2} minute, {value.Seconds:D2} seconds, {value.Milliseconds:D2} milliseconds");
    }
}