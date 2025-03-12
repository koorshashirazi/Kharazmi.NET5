#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Kharazmi.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty([NotNullWhen(false)] this string? value)
            => string.IsNullOrWhiteSpace(value);

        public static bool IsNotEmpty([NotNullWhen(true)] this string? value)
            => !IsEmpty(value);


        public static int ExtractNumber([NotNull] this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            var match = Regex.Match(value, "[0-9]+", RegexOptions.IgnoreCase);
            return match.Success ? match.Value.ToInt() : 0;
        }

        public static int ToInt([NotNull] this string value, bool throwExceptionIfFailed = false)
        {
            var valid = int.TryParse(value, out var result);
            if (!valid)
                if (throwExceptionIfFailed)
                    throw new FormatException($"'{value}' cannot be converted as int");

            return result;
        }

        public static double ToDouble([NotNull] this string value, bool throwExceptionIfFailed = false)
        {
            var valid = double.TryParse(value, out var result);
            if (!valid)
                if (throwExceptionIfFailed)
                    throw new FormatException($"'{value}' cannot be converted as double");

            return result;
        }

        public static string Reverse([NotNull] this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static bool IsNumber([NotNull] this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            var match = Regex.Match(value, pattern: @"^[0-9]+$", RegexOptions.IgnoreCase);
            return match.Success;
        }

        public static TimeSpan ToTimeSpan([NotNull] this string timeSpanFormat)
        {
            if (timeSpanFormat.IsEmpty()) return TimeSpan.Zero;
            var times = timeSpanFormat.Split(',');
            var tdResult = Convert.ToInt32(times[0]);
            var thResult = Convert.ToInt32(times[1]);
            var tmResult = Convert.ToInt32(times[2]);
            var tsResult = Convert.ToInt32(times[3]);
            var tmsResult = Convert.ToInt32(times[4]);
            var time = new TimeSpan(
                tdResult,
                thResult,
                tmResult,
                tsResult,
                tmsResult);

            return time;
        }

        public static string ToLowerUnderscore([NotNull] this string value, char separator = '.')
        {
            var str1 = value.Split(separator);
            var str2 = str1.Select(s => s.ToLower().ToLowerWithFirstUpper());
            var str3 = string.Concat(str2).ToUnderscore();
            return str3;
        }

        public static string ToUnderscore([NotNull] this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()));

        public static string ToLowerWithFirstUpper([NotNull] this string input)
            => $"{input.First().ToString().ToUpper()} {input[1..].ToLower()}";

        public static string FirstToUpper([NotNull] this string input) =>
            $"{input.First().ToString().ToUpper()} {input[1..]}";

        public static string FirstToLower(this string input) => input.First().ToString().ToLower() + input[1..];
    }
}