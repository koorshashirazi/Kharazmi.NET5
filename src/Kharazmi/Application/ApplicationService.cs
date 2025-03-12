using System.Collections.Generic;
using Kharazmi.Functional;
using Kharazmi.Models;

namespace Kharazmi.Application
{
    public abstract class ApplicationService : IApplicationService
    {
        protected static Result Ok() => Result.Ok();
        protected static Result Fail(string message) => Result.Fail(message);

        protected static Result Fail(string message, IEnumerable<ValidationFailure> failures) =>
            Result.Fail(message).WithValidationMessages(failures);

        protected static Result<T> Ok<T>(T value) => Result.OkAs(value);
        protected static Result<T> Fail<T>(string message) => Result.FailAs<T>(message);

        protected static Result<T> Fail<T>(string message, IEnumerable<ValidationFailure> failures) =>
            Result.FailAs<T>(message).WithValidationMessages(failures);
    }
}