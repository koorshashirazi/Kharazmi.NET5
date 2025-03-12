#region

using System;
using System.Collections.Generic;
using Kharazmi.Models;

#endregion

namespace Kharazmi.Validations
{
    public interface IValidationHandler<in TModel> : IValidationHandler
    {
        IEnumerable<ValidationFailure> Validate(TModel model);
    }

    public interface IValidationHandler
    {
        IEnumerable<ValidationFailure> Validate(object model);
        bool CanValidateOfType(Type type);
    }
}