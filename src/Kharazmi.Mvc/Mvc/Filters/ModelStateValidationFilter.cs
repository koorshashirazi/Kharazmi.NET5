#region

using System.Diagnostics;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Mvc.Mvc.Filters
{
    [DebuggerStepThrough]
    public class ModelStateValidationFilter : IAsyncActionFilter
    {
        private const string DefaultErrorViewName = "Error";

        public string ViewName { get; }

        public ModelStateValidationFilter(string viewName)
        {
            ViewName = viewName;
        }


        [DebuggerStepThrough]
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var controllerName = actionDescriptor?.ControllerName;
                var actionName = actionDescriptor?.ActionName;
                var errorResult = context.ModelState.ToErrorResult();
                var logger = context.HttpContext.RequestServices.GetService<ILogger<ModelStateValidationFilter>>();

                logger?.LogError(
                    "Invalid ModelState with controller: {ControllerName}, action: {ActionName}, result: {Result}",
                    controllerName, actionName, errorResult.ToString());

                var isAjax = context.HttpContext.Request.IsAjaxRequest();
                var controller = context.Controller as Controller;

                if (!isAjax)
                {
                    var tempData = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>()
                        .GetTempData(context.HttpContext);
                    var modelMetadataProvider =
                        context.HttpContext.RequestServices.GetService<IModelMetadataProvider>();

                    var viewResult = new ViewResult
                    {
                        ViewName = ViewName.IsEmpty()
                            ? DefaultErrorViewName
                            : ViewName,
                        TempData = controller?.TempData ?? tempData,
                        ViewData = controller?.ViewData ??
                                   new ViewDataDictionary(modelMetadataProvider, context.ModelState)
                    };

                    viewResult.ViewData.Model = errorResult;
                    context.Result = viewResult;
                    return;
                }


                context.Result = new BadRequestObjectResult(errorResult);
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                return;
            }

            await next();
        }
    }
}