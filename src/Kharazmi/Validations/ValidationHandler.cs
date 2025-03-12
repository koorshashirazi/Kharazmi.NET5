#region

using System;
using System.Collections.Generic;
using System.Reflection;
using Kharazmi.Exceptions;
using Kharazmi.Guard;
using Kharazmi.Models;

#endregion

namespace Kharazmi.Validations
{
    public abstract class ValidationHandler<TModel> : IValidationHandler<TModel>
    {
        IEnumerable<ValidationFailure> IValidationHandler.Validate(object model)
        {
            model.NotNull(nameof(model));
            if (((IValidationHandler) this).CanValidateOfType(model.GetType())) return Validate((TModel) model);

            var objectName = model.GetType().GetTypeInfo().Name;
            var modelName = typeof(TModel).Name;
            throw ValidationException.For(
                $"Cannot validate instances of type '{objectName}'. This validator can only validate instances of type '{modelName}'.",
                new InvalidOperationException());
        }

        bool IValidationHandler.CanValidateOfType(Type type)
        {
            return typeof(TModel).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        protected IEnumerable<ValidationFailure> Empty => NullValidationHandler<TModel>.Empty;

        public abstract IEnumerable<ValidationFailure> Validate(TModel model);
    }
}