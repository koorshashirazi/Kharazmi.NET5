#region

using System;
using FluentValidation;

#endregion

namespace Kharazmi.Validation
{

    internal class ValidatorFactory : ValidatorFactoryBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidatorFactory(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public override IValidator? CreateInstance(Type validatorType)
            => _serviceProvider.GetService(validatorType) as IValidator;
    }
}