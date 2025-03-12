#region

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Handlers;
using Kharazmi.Models;
using Kharazmi.Mvc.Extensions;
using Kharazmi.Notifications;
using Kharazmi.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

#endregion

namespace Kharazmi.Mvc.Exceptions
{
    public class GlobalExceptionActionFilter : ExceptionFilterAttribute
    {
        private const string DefaultErrorViewName = "Error";
        private const string DefaultActionName = "Index";
        private const string DefaultControllerName = "Errors";

        private ILogger<GlobalExceptionActionFilter>? _logger;
        private ITempDataDictionaryFactory? _tempDataDictionaryFactory;
        private IModelMetadataProvider? _modelMetadataProvider;

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            var services = context.HttpContext.RequestServices;
            _logger = services.GetService<ILogger<GlobalExceptionActionFilter>>();
            _tempDataDictionaryFactory = services.GetService<ITempDataDictionaryFactory>();
            _modelMetadataProvider = services.GetService<IModelMetadataProvider>();
            return ProcessExceptionAsync(context, services);
        }


        protected virtual Task ProcessExceptionAsync(ExceptionContext context, IServiceProvider services)
        {
            LogGlobalException(context);
            LogFrameworkException(context);

            if (!(context.ActionDescriptor is ControllerActionDescriptor))
                return base.OnExceptionAsync(context);

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor is null)
                return base.OnExceptionAsync(context);

            Type controllerType = actionDescriptor.ControllerTypeInfo;
            var isAjax = context.HttpContext.Request.IsAjaxRequest();
            var tempData = _tempDataDictionaryFactory?.GetTempData(context.HttpContext);

            SelectHandlerException(context);

            // Api
            if (controllerType.IsApiController())
            {
            }

            // Mvc
            if (controllerType.IsMvcController())
            {
                LogModelState(context);

                if (!isAjax)
                {
                    base.OnException(context);

                    var settings = services.GetRequiredService<ISettingProvider>();

                    var exceptionOption = settings.Get<ExceptionOption>();
                  
                    var viewResult = new ViewResult
                    {
                        ViewName = exceptionOption.ErrorViewName.IsEmpty()
                            ? DefaultErrorViewName
                            : exceptionOption.ErrorViewName,
                        TempData = tempData,
                        ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                    };

                    var objectResult = context.Result as BadRequestObjectResult;
                    var result = objectResult?.Value as ExecuteResult;

                    if (exceptionOption.UseTempDataNotification)
                    {
                        if (result != null)
                        {
                            if (result.Reason.IsNotEmpty())
                                tempData?.AddNotification(NotificationOptions.For(NotificationType.Error,
                                    result.Reason, result.Code ?? ""));

                            if (result.Messages != null)
                                foreach (var resultMessage in result.Messages)
                                    if (resultMessage.Description.IsNotEmpty())
                                        tempData?.AddNotification(NotificationOptions.For(NotificationType.Error,
                                            resultMessage.Description, resultMessage.Code ?? ""));
                            if (result.ValidationMessages != null)
                                foreach (var validationMessage in result.ValidationMessages)
                                    tempData?.AddNotification(NotificationOptions.For(NotificationType.Validation,
                                        validationMessage.ErrorMessage, validationMessage.PropertyName));
                        }

                        if (tempData?[NotificationConstant.DomainNotification] != null)
                            viewResult.ViewData.Add(
                                NotificationConstant.Notifications,
                                tempData[NotificationConstant.DomainNotification]
                            );
                    }

                    viewResult.ViewData.Model = result;
                    context.ExceptionHandled = true;
                    context.Result = viewResult;

                    if (exceptionOption?.RedirectOptions != null)
                    {
                        var redirectOptions = exceptionOption.RedirectOptions;
                        var redirectResult = redirectOptions.ActionName.IsEmpty() ||
                                             redirectOptions.ControllerName.IsEmpty()
                            ? new RedirectToActionResult(DefaultActionName, DefaultControllerName,
                                new {isAjaxRequest = false})
                            : new RedirectToActionResult(redirectOptions.ActionName,
                                redirectOptions.ControllerName,
                                redirectOptions.RouteData);
                        context.Result = redirectResult;
                    }
                }
                else
                {
                    context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                }
            }


            Dispose(context, tempData);

            return Task.CompletedTask;
        }

        private void SelectHandlerException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ValidationException validationException:
                    HandleValidationError(context, validationException);
                    break;
                case DomainException domainException:
                    HandleDomainError(context, domainException);
                    break;
                default:
                    HandleProblemDetails(context);
                    break;
            }
        }


        protected virtual void LogGlobalException(ExceptionContext context)
        {
            var ex = context.HttpContext.Features?.Get<IExceptionHandlerFeature>()?.Error;
            var errors = ex?.CollectExceptions();
            if (errors is null) return;
            foreach (var error in errors)
                if (error.Description.IsNotEmpty())
                    _logger.LogError(
                        "Global Exception with code {Code}, description {Description}", error.Code,
                        error.Description);
        }

        protected virtual void LogFrameworkException(ExceptionContext context)
        {
            _logger.LogTrace("{DisplayName}", context.ActionDescriptor?.DisplayName);
            
            switch (context.Exception)
            {
                case DomainException domainException:
                {
                    var result = domainException.Result
                        .WithStatus(StatusCodes.Status400BadRequest)
                        .WithRequestPath(context.HttpContext.Request.Path)
                        .WithTraceId(Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

                    _logger.LogError("{Result}", result.ToString());

                    break;
                }
                case ValidationException validationException:
                {
                    _logger.LogError("Validation exception with code {Code} , message {Message}",
                        validationException.Code, validationException.Message);

                    foreach (var exception in validationException.Failures)
                        _logger.LogError("Validation exception with property name {PropertyName} , error message {ErrorMessage}",
                            exception.PropertyName, exception.ErrorMessage);
                    break;
                }
                default:
                    if (context.Exception is FrameworkException frameworkException)
                        _logger.LogError("Framework exception with code {Code} , message {Message}",
                            frameworkException.Code, frameworkException.Message);

                    break;
            }
        }

        protected virtual void LogModelState(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case DomainException executeResult:
                {
                    if (executeResult.Result.ValidationMessages is not null)
                        foreach (var message in executeResult.Result.ValidationMessages)
                        {
                            context.ModelState.AddModelError(message.PropertyName, message.ErrorMessage);
                            _logger.LogError("Model state with property name {PropertyName} , error message {ErrorMessage}",
                                message.PropertyName, message.ErrorMessage);
                        }

                    break;
                }
                case ValidationException validationException:
                    foreach (var message in validationException.Failures)
                    {
                        context.ModelState.AddModelError(message.PropertyName, message.ErrorMessage);
                        _logger.LogError("Model state with property name {PropertyName} , error message {ErrorMessage}}",
                            message.PropertyName, message.ErrorMessage);
                    }

                    break;
            }
        }

        protected virtual void HandleDomainError(ExceptionContext context, DomainException domainException)
        {
            context.Result = new BadRequestObjectResult(new ExecuteResult
            {
                Reason = "Bad Request",
                ValidationMessages = domainException.Result.ValidationMessages,
                Messages = domainException.Result.Messages,
                Status = StatusCodes.Status400BadRequest
            })
            {
                ContentTypes = new MediaTypeCollection
                {
                    new MediaTypeHeaderValue("application/json")
                }
            };
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.ExceptionHandled = true;
        }

        protected virtual void HandleValidationError(ExceptionContext context,
            ValidationException validationException)
        {
            context.Result = new BadRequestObjectResult(new ExecuteResult
            {
                Reason = "Validation failures",
                ValidationMessages = validationException.Failures,
                Status = StatusCodes.Status400BadRequest
            })
            {
                ContentTypes = new MediaTypeCollection
                {
                    new MediaTypeHeaderValue("application/json")
                }
            };
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.ExceptionHandled = true;
        }

        protected virtual void HandleProblemDetails(ExceptionContext context)
        {
            if (context.Exception is DomainException ||
                context.Exception is ValidationException) return;

            context.Result = new BadRequestObjectResult(new ExecuteResult
            {
                Reason = "There was an error.",
                Status = StatusCodes.Status400BadRequest
            })
            {
                ContentTypes = new MediaTypeCollection
                {
                    new MediaTypeHeaderValue("application/json")
                }
            };
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.ExceptionHandled = true;
        }

        protected virtual void ClearNotification(ITempDataDictionary? tempData)
        {
            tempData?.RemoveNotifications();
        }

        protected virtual void Dispose(ExceptionContext context, ITempDataDictionary? tempData)
        {
            ClearNotification(tempData);
            context.HttpContext.RequestServices?.GetService<IDomainNotificationHandler>()?.Reset();

            switch (context.Exception)
            {
                case ValidationException validationException:
                    validationException.Dispose();
                    break;
            }
        }
    }
}