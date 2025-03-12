#region

using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Dependency;
using Kharazmi.Models;

#endregion

namespace Kharazmi.Validations
{
    public class NullValidationHandler<TModel> : IValidationHandler<TModel>, INullInstance
    {
        public IEnumerable<ValidationFailure> Validate(TModel model)
            => Enumerable.Empty<ValidationFailure>();

        public IEnumerable<ValidationFailure> Validate(object model)
            => Enumerable.Empty<ValidationFailure>();

        public bool CanValidateOfType(Type type) => true;

        public static IEnumerable<ValidationFailure> Empty => new NullValidationHandler<TModel>().Validate(null!);
    }
}