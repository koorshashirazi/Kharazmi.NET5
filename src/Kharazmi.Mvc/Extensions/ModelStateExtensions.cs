﻿#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Mvc.Extensions
{
    public static class ModelStateExtensions
    {
        public static void ClearError<T>(this ModelStateDictionary modelState, T model, string nameOfPropertyModel = "")
            where T : class, new()
        {
            var properties = model.GetType().GetProperties();
            foreach (var item in properties)
            {
                var key = string.IsNullOrWhiteSpace(nameOfPropertyModel)
                    ? item.Name
                    : $"{nameOfPropertyModel}.{item.Name}";
                if (!modelState.ContainsKey(key)) continue;

                if (modelState[key].Errors.Any()) modelState.Remove(key);
            }
        }

        public static string GetError(this ModelStateDictionary modelState)
        {
            return string.Join("; ", modelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
        }

        public static List<string> GetErrorList(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
        }

        public static List<ModelStateViewModel> GetModelStateViewModels(this ModelStateDictionary modelState)
        {
            return modelState.GetErrorList()
                .Select(error => new ModelStateViewModel {Type = ModelStateType.Error, Message = error}).ToList();
        }

        /// <summary>
        /// Converts the <paramref name="modelState"/> to a dictionary that can be easily serialized.
        /// </summary>
        public static IDictionary<string, string[]> ToSerializableDictionary(this ModelStateDictionary modelState)
        {
            return modelState.Where(x => x.Value.Errors.Any()).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        }

        /// <summary>
        /// Stores the errors in a ModelValidationResult object to the specified modelstate dictionary.
        /// </summary>
        /// <param name="result">The validation result to store</param>
        /// <param name="modelState">The ModelStateDictionary to store the errors in.</param>
        /// <param name="prefix">An optional prefix. If ommitted, the property names will be the keys. If specified, the prefix will be concatenatd to the property name with a period. Eg "user.Key"</param>
        public static void AddModelError([NotNull] this ModelStateDictionary modelState, [NotNull] Result result,
            string? prefix = null)
        {
            if (!result.Failed) return;

            if (result.Description.IsNotEmpty())
                modelState.AddModelError(prefix ?? result.Code ?? "", result.Description ?? "");

            if (result.Messages != null)
                foreach (var identityError in result.Messages)
                    modelState.AddModelError(identityError.Code ?? "", identityError.Description ?? "");

            if (result.ValidationMessages is null) return;
            foreach (var failure in result.ValidationMessages)
            {
                var key = prefix.IsEmpty() || failure.PropertyName.IsEmpty()
                    ? failure.PropertyName
                    : prefix + "." + failure.PropertyName;

                if (!modelState.ContainsKey(key) ||
                    modelState[key].Errors.All(i => i.ErrorMessage != failure.ErrorMessage))
                    modelState.AddModelError(key, failure.ErrorMessage);
            }
        }

        public static string ExportErrors(this ModelStateDictionary modelState, bool useHtmlNewLine = false)
        {
            var builder = new StringBuilder();

            foreach (var error in modelState.Values.SelectMany(a => a.Errors))
            {
                var message = error.ErrorMessage;
                if (string.IsNullOrWhiteSpace(message)) continue;

                builder.AppendLine(!useHtmlNewLine ? message : $"{message}<br/>");
            }

            return builder.ToString();
        }


        public static void ExportModelStateToTempData(this ModelStateDictionary? modelState, Controller? controller,
            string key)
        {
            if (controller is null || modelState is null) return;
            var modelStateJson = SerializeModelState(modelState);
            controller.TempData[key] = modelStateJson;
        }

        public static string SerializeModelState(this ModelStateDictionary modelState)
        {
            var values = modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp.Value.Errors.Select(err => err.ErrorMessage).ToList(),
                });

            return JsonConvert.SerializeObject(values);
        }

        public static IEnumerable<ModelStateTransferValue> GetModelStateTransferValue(
            this ModelStateDictionary modelState)
        {
            return modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp.Value.Errors.Select(err => err.ErrorMessage).ToList(),
                }).ToList();
        }

        public static Result ToErrorResult(this ModelStateDictionary modelState)
        {
            return Result.Fail("").WithValidationMessages(modelState.ToValidationFailure());
        }

        public static List<ValidationFailure> ToValidationFailure(this ModelStateDictionary modelState)
        {
            return modelState
                .SelectMany(kvp => kvp.Value.Errors, (kv, error) =>
                    ValidationFailure.For(kv.Key, error.ErrorMessage)).ToList();
        }

        public static Result ToResult(this ModelStateDictionary modelState)
        {
            return Result.Fail("").WithValidationMessages(modelState.ToValidationFailure());
        }
    }
}