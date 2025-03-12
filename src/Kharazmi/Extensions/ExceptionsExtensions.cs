#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Exceptions;
using Kharazmi.Functional;
using Kharazmi.Helpers;
using Kharazmi.Models;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Extensions
{
    public class StackFrameModel
    {
        /// <summary> </summary>
        public string? FileName { get; set; }

        /// <summary></summary>
        public string? ClassName { get; set; }

        /// <summary></summary>
        public string? MethodName { get; set; }

        /// <summary></summary>
        public int LinNumber { get; set; }
    }

    public class DetailsError
    {
        /// <summary></summary>
        public string? CurrentDate { get; set; }

        /// <summary> </summary>
        public IEnumerable<StackFrameModel>? StackFrameModel { get; set; }

        /// <summary> </summary>
        public IEnumerable<MessageModel>? ExceptionErrors { get; set; }
    }

    public static class ExceptionsExtensions
    {
        /// <summary>_</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exception"></param>
        /// <param name="currentType"></param>
        /// <returns></returns>
        public static T? ToJsonException<T>([NotNull] this T exception, [NotNull] Type currentType)
            where T : Exception, new()
        {
            var stacktrace = currentType.GetStackTraces();

            var stackFrameModels = stacktrace.Select(stackFrame => stackFrame.GetStackFrame()).ToList();

            var details = new DetailsError
            {
                StackFrameModel = stackFrameModels,
                CurrentDate = DateTimeHelper.DateTimeOffsetUtcNow.UtcDateTime.ToShortDateString(),
                ExceptionErrors = exception.CollectExceptions()
            };

            var jsonDetails = JsonConvert.SerializeObject(details, Formatting.Indented, new JsonSerializerSettings
            {
                MaxDepth = 10,
                NullValueHandling = NullValueHandling.Ignore
            });

            return Activator.CreateInstance(typeof(T), jsonDetails, exception) as T;
        }

        /// <summary>_</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IEnumerable<MessageModel> CollectExceptions<T>([NotNull]this T e)
            where T : Exception, new()
        {
            var exceptionErrors = new List<MessageModel>();
            while (true)
            {
                exceptionErrors.Add(MessageModel.For(e.Message, typeof(T).Name));
                if (e.InnerException is null) return exceptionErrors;
                exceptionErrors.Add(MessageModel.For("\r\nInnerException: " + e.InnerException.Message,
                    e.GetType().Name));
                e = (T) e.InnerException;
            }
        }

        public static void AsFrameworkException(this Exception exception)
        {
            throw new  FrameworkException(exception.Message, exception);
        }
        
        public static void AsDomainException(this Exception exception)
        {
            switch (exception)
            {
                case DomainException domainException:
                    throw domainException;
                case ValidationException validationException:
                    var validException = DomainException.For(Result.Fail(validationException.Message)
                        .WithMessages(exception.CollectExceptions()));
                    throw validException;
                case OperationCanceledException canceledException:

                    var operationCanceledException = DomainException.For(Result.Fail(canceledException.Message)
                        .WithMessages(exception.CollectExceptions()));
                    throw operationCanceledException;
                default:
                    var otherExceptions =
                        DomainException.For(Result.Fail("").WithMessages(exception.CollectExceptions()));
                    throw otherExceptions;
            }
        }

        public static DomainException ToDomainException(this Exception exception)
        {
            switch (exception)
            {
                case DomainException domainException:
                    return domainException;
                case ValidationException validationException:
                    var validException = DomainException.For(Result
                        .Fail(validationException.Description, validationException.Code)
                        .WithValidationMessages(validationException.Failures), validationException);
                    return validException;
                case OperationCanceledException canceledException:
                    var operationCanceledException =
                        DomainException.For(Result.Fail(canceledException.Message), exception);
                    return operationCanceledException;
                default:
                    var otherExceptions = DomainException.For(Result.Fail(exception.Message), exception);
                    return otherExceptions;
            }
        }

        public static DomainException? ToDomainException([NotNull]this FrameworkException exception)
            => exception as DomainException;

        public static ValidationException? ToValidationException([NotNull]this FrameworkException exception)
            => exception as ValidationException;

        public static StackFrameModel GetStackFrame(this StackFrame stackFrame)
        {
            var methodBase = stackFrame.GetMethod();
            var fileName = stackFrame.GetFileName();
            var lineNumber = stackFrame.GetFileLineNumber();
            var memberInfo = methodBase?.DeclaringType;
            return new StackFrameModel
            {
                FileName = fileName,
                ClassName = memberInfo?.FullName,
                MethodName = methodBase?.Name,
                LinNumber = lineNumber
            };
        }
    }
}