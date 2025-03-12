#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Models;
using Kharazmi.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace Kharazmi.Mvc.Mvc
{
    public abstract class FrameworkController : Controller
    {
        protected DomainException DomainException { get; set; }

        protected FrameworkController()
            => DomainException = DomainException.Empty();

        protected virtual void AddModelError([NotNull] MessageModel error)
        {
            if (error.Description.IsNotEmpty())
                ModelState.AddModelError(error.Code ?? "", error.Description);
        }

        protected virtual void AddModelErrors([NotNull] List<MessageModel> errors)
        {
            foreach (var error in errors)
            {
                if (error.Description.IsNotEmpty())
                    ModelState.AddModelError(error.Code ?? "", error.Description);
            }
        }

        protected virtual void AddModelErrors([NotNull] List<ValidationFailure> errors)
        {
            foreach (var error in errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        protected virtual void AddError(Result result)
            => DomainException = DomainException.For(result);

        protected virtual void ExecuteFail()
            => throw DomainException;

        public virtual Result ModelStResult => ModelState.ToResult();

        protected virtual DomainException ExecuteFail([NotNull] Result result)
            => DomainException.For(result);
    }
}