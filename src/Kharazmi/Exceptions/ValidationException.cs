#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Models;

#endregion

namespace Kharazmi.Exceptions
{
    /// <summary>_</summary>
    [Serializable]
    public class ValidationException : FrameworkException
    {
        private IReadOnlyCollection<ValidationFailure> _failures = new List<ValidationFailure>();

        /// <summary> </summary>
        public IReadOnlyCollection<ValidationFailure> Failures
        {
            get => _failures;
            protected set => _failures = value;
        }


        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>_</summary>
        public ValidationException() : this("", null, "", "", Enumerable.Empty<ValidationFailure>())
        {
        }


        /// <summary>_</summary>
        /// <param name="code"></param>
        /// <param name="innerException"></param>
        /// <param name="errors"></param>
        /// <param name="description"></param>
        /// <param name="message"></param>
        protected ValidationException(
            string? message, Exception? innerException,
            string? description, string? code,
            IEnumerable<ValidationFailure>? errors) : base(message, innerException, description, code)
        {
            Failures = errors?.AsReadOnly() ?? Enumerable.Empty<ValidationFailure>().AsReadOnly();
        }


        /// <summary>_</summary>
        /// <returns></returns>
        public static ValidationException Empty()
        {
            var type = GeValidType();

            return new ValidationException().ToJsonException(type) ?? new ValidationException();
        }

        private static Type GeValidType()
        {
            var type = new StackTrace().GetFrames()[1].GetMethod()?.DeclaringType ?? typeof(ValidationException);
            return type;
        }


        public static ValidationException For(string? message, Exception? exception = null)
        {
            var type = GeValidType();
            return new ValidationException().ToJsonException(type) ?? new ValidationException();
        }

        public static ValidationException For([NotNull] Type type, string message, Exception? exception = null)
        {
            return For(message, exception).ToJsonException(type) ?? For(message, exception);
        }


        /// <summary>_</summary>
        /// <param name="result"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static ValidationException? From([NotNull] Result result, string? message = "",
            Exception? exception = null)
        {
            var ex = For(message, exception)
                .AddFailureMessages(result.ValidationMessages)
                .WithCode(result.Code)
                .WithDescription(result.Description);
            
            return ex.ToValidationException();
        }


        /// <summary>_</summary>
        /// <param name="failure"></param>
        /// <returns></returns>
        public ValidationException AddFailureMessage([NotNull] ValidationFailure failure)
        {
            var failureMessages = Failures.ToList();
            failureMessages.Add(failure);
            Failures = failureMessages.AsReadOnly();

            return this;
        }

        /// <summary>_</summary>
        /// <param name="failures"></param>
        /// <returns></returns>
        public ValidationException AddFailureMessages(IEnumerable<ValidationFailure>? failures)
        {
            var failureMessages = Failures.ToList();
            if (failures != null)
                failureMessages.AddRange(failures);
            Failures = failureMessages.AsReadOnly();

            return this;
        }
    }
}