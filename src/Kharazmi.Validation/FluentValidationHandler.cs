#region

using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Kharazmi.Guard;
using Kharazmi.Models;
using Kharazmi.Validations;

#endregion

namespace Kharazmi.Validation
{

    internal class FluentValidationHandler<TModel> : ValidationHandler<TModel>
    {
        private readonly IValidatorFactory _validatorFactory;

        public FluentValidationHandler(IValidatorFactory validatorFactory)
           => _validatorFactory = validatorFactory.NotNull(nameof(validatorFactory));

        public override IEnumerable<ValidationFailure> Validate(TModel model)
        {
            if (model is null) return Empty;

            if (!((IValidationHandler) this).CanValidateOfType(model.GetType()))
            {
                return Empty;
            }

            var validator = _validatorFactory.GetValidator(model.GetType());
            if (validator is null) return Empty;

            var validationResult = validator.Validate(new ValidationContext<TModel>(model));
            var failure = validationResult.Errors
                .Select(x => new ValidationFailure(x.PropertyName, x.ErrorMessage).WithAttemptedValue(x.AttemptedValue))
                .ToList();

            return failure;
        }
    }
}