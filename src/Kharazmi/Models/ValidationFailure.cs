#region

using System;
using System.Collections.Generic;
using System.Text;
using Kharazmi.Functional;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Models
{
    /// <summary>Defines a validation failure</summary>
    [Serializable]
    public class ValidationFailure
    {
        /// <summary>Creates a new ValidationFailure.</summary>
        [JsonConstructor]
        public ValidationFailure(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        public static ValidationFailure For(string propertyName, string errorMessage)
        {
            return new(propertyName, errorMessage);
        }

        public ValidationFailure WithErrorMessage(string errorMessage)
        {
            ErrorMessage = errorMessage;
            return this;
        }

        public ValidationFailure WithPropertyName(string propertyName)
        {
            PropertyName = propertyName;
            return this;
        }

        /// <summary>The name of the property.</summary>
        [JsonProperty]
        public string PropertyName { get; set; }

        /// <summary>The error message</summary>
        [JsonProperty]
        public string ErrorMessage { get; set; }

        /// <summary>The property value that caused the failure.</summary>
        [JsonIgnore]
        public object? AttemptedValue { get; set; }

        /// <summary>Custom state associated with the failure.</summary>
        [JsonIgnore]
        public object? CustomState { get; set; }

        /// <summary>Custom severity level associated with the failure.</summary>
        [JsonIgnore]
        public Severity Severity { get; set; }

        /// <summary>Gets or sets the error code.</summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the formatted message arguments.
        /// These are values for custom formatted message in validator resource files
        /// Same formatted message can be reused in UI and with same number of format placeholders
        /// Like "Value {0} that you entered should be {1}"
        /// </summary>
        [JsonIgnore]
        public object[]? FormattedMessageArguments { get; set; }

        /// <summary>Gets or sets the formatted message placeholder values.</summary>
        [JsonIgnore]
        public Dictionary<string, object>? FormattedMessagePlaceholderValues { get; set; }

        /// <summary>The resource name used for building the message</summary>
        [JsonIgnore]
        public string? ResourceName { get; set; }

        /// <summary>Creates a textual representation of the failure.</summary>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append("#ValidationFailure with: ");
            builder.Append($"{{PropertyName}}: {PropertyName}");
            builder.Append($"{{ErrorMessage}}: {ErrorMessage}");

            return builder.ToString();
        }

        public ValidationFailure WithAttemptedValue(object attemptedValue)
        {
            AttemptedValue = attemptedValue;
            return this;
        }
    }
}