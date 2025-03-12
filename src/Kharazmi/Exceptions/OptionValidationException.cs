using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Kharazmi.Exceptions
{
    public class OptionValidationException : Exception
    {
        public OptionValidationException(string message) : base(message)
        {
        }

        public OptionValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static OptionValidationException Create(string message)
        {
            return new OptionValidationException(message);
        }

        /// <summary>/ </summary>
        public static OptionValidationException Empty => new("");

        public static OptionValidationException From(string optionNam, IEnumerable<ValidationResult> result)
        {
            var messages = string.Join("; ", result.Select(x => x.ErrorMessage));
            return new($"Validate a options with name {optionNam}, Validation results: {messages}");
        }
    }
}