#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Models;

#endregion

namespace Kharazmi.Exceptions
{
    [Serializable]
    public class DomainException : FrameworkException
    {
        private List<MessageModel> _exceptionErrors = new();
        public Result Result { get; } = Result.Fail("");

        public IReadOnlyCollection<MessageModel> ExceptionErrors => _exceptionErrors;

        public DomainException()
        {
        }


        public DomainException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        private DomainException(string? message, Exception? exception, Result result,
            IEnumerable<MessageModel>? exceptionErrors) : base(message, exception)
        {
            Result = result;
            if (exceptionErrors != null)
                _exceptionErrors = exceptionErrors.ToList();
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public static DomainException Empty()
        {
            return new DomainException().ToJsonException(typeof(DomainException).GetLastCalledType()) ??
                   new DomainException();
        }

        public static DomainException For([NotNull] Result result)
            => new(result.ToString(), null, result, Enumerable.Empty<MessageModel>());

        public static DomainException For([NotNull] Result result, Exception exception, Type? type = null)
        {
            var detailsJsonException =
                exception.ToJsonException(type ?? typeof(DomainException).GetLastCalledType());

            return new DomainException(detailsJsonException?.Message, detailsJsonException, result,
                Enumerable.Empty<MessageModel>());
        }

        public DomainException AddExceptionMessage([NotNull] Exception exception)
        {
            _exceptionErrors.AddRange(exception.CollectExceptions());
            return this;
        }

        public DomainException AddExceptionMessage([NotNull] MessageModel error)
        {
            _exceptionErrors.Add(error);
            return this;
        }


        public DomainException AddExceptionMessages([NotNull] IEnumerable<MessageModel> errors)
        {
            _exceptionErrors.AddRange(errors);
            return this;
        }
    }
}