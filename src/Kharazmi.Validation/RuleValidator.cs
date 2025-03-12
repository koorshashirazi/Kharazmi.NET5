#region

using FluentValidation;

#endregion

namespace Kharazmi.Validation
{
    public abstract class RuleValidator<T> : AbstractValidator<T>
    {
    }
}