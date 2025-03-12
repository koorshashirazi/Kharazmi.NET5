#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;

#endregion

namespace Kharazmi.Guard
{
    public static class Ensure
    {
        public static TArgument NotNull<TArgument>(this TArgument? argument, string paramName)
            where TArgument : class
        {
            var type = GetStackType();
            return argument ?? throw new ArgumentTypeNullException(paramName).ToJsonException(type) ??
                                     throw new ArgumentTypeNullException(paramName);
        }

        public static TArgument NotNull<TArgument>([NotNull]this Type callMethodBaseClass,
            [NotNullWhen(true)] TArgument? argument, string parameterName)
        {
            return argument ??
                   throw new ArgumentTypeNullException(parameterName).ToJsonException(callMethodBaseClass) ??
                         throw new ArgumentTypeNullException(parameterName);
        }

        public static string NotEmpty(this string? argument, string parameterName)
        {
            var type = new StackTrace().GetFrames()[1].GetMethod()?.DeclaringType ?? typeof(Ensure);
            return string.IsNullOrEmpty(argument)
                ? throw new ArgumentEmptyException(parameterName).ToJsonException(type) ??
                        throw new ArgumentEmptyException(parameterName)
                : argument;
        }

        public static string NotEmpty([NotNull]this Type type, string? argument, string parameterName)
        {
            return string.IsNullOrEmpty(argument)
                ? throw new ArgumentEmptyException(type, parameterName).ToJsonException(type) ??
                        throw new ArgumentEmptyException(type, parameterName)
                : argument;
        }

        private static Type GetStackType() =>
            new StackTrace().GetFrames()[1].GetMethod()?.DeclaringType ?? typeof(Ensure);
    }
}