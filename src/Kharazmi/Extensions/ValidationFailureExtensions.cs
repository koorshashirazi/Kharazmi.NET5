#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Functional;
using Kharazmi.Models;

#endregion

namespace Kharazmi.Extensions
{
    public static class ValidationFailureExtensions
    {
        public static Result ToResult([NotNull] this IEnumerable<ValidationFailure> failures)
        {
            failures = failures.ToList();

            return !failures.Any()
                ? Result.Ok()
                : Result.Fail(string.Empty).WithValidationMessages(failures);
        }

        public static Result ToResult([NotNull] this List<ValidationFailure> failures)
        {
            return !failures.Any()
                ? Result.Ok()
                : Result.Fail(string.Empty).WithValidationMessages(failures);
        }

        public static string AsString(this IEnumerable<ValidationResult> validationResults)
            => string.Join(", ", validationResults);
    }
}