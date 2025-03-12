using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kharazmi.Options
{
    public interface INestedOption
    {
        bool IsValid();
        IReadOnlyCollection<ValidationResult> ValidationResults();
        void Validate();
    }

    public abstract class NestedOption : INestedOption
    {
        private readonly List<ValidationResult> _validation = new();

        protected NestedOption()
        {
        }

        public bool IsValid() => _validation.Count == 0;

        public IReadOnlyCollection<ValidationResult> ValidationResults() => _validation;

        protected void AddValidation(string errorMessage, params string[] memberName)
            => _validation.Add(new ValidationResult(errorMessage, memberName));

        protected void AddValidation(ValidationResult value)
            => _validation.Add(value);

        protected void AddValidations(IEnumerable<ValidationResult> values)
            => _validation.AddRange(values);

        public abstract void Validate();
    }
}