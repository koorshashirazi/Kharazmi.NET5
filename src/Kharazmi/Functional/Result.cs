#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Kharazmi.Extensions;
using Kharazmi.Helpers;
using Kharazmi.Models;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Functional
{
    /// <summary></summary>
    [Serializable]
    public class Result : IDisposable
    {
        protected List<MessageModel>? MessageModels;
        protected List<ValidationFailure>? Failures;
        private readonly object? _value;

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <param name="failed"></param>
        /// <param name="code"></param>
        /// <param name="messages"></param>
        /// <param name="failures"></param>
        /// <param name="description"></param>
        [JsonConstructor]
        internal Result(
            object? value,
            bool failed,
            string? description,
            string? code,
            List<MessageModel>? messages,
            List<ValidationFailure>? failures)
        {
            _value = value;
            ResultId = Guid.NewGuid().ToString("N");
            CreateAt = DateTimeHelper.DateTimeOffsetUtcNow.UtcDateTime.ToString("g");

            Failures = failures ?? new List<ValidationFailure>();
            MessageModels = messages ?? new List<MessageModel>();

            Failed = Failures.Any() || failed;
            Description = description;
            Code = code;

            ResultStatus = failed && !Failures.Any() ? "Error" :
                Failures.Any() ? "Failure" :
                !failed && !Failures.Any() ? "Success" : "None";
        }

        /// <summary> </summary>
        public object? Value => !Failed && _value != null ? _value : default;

        public bool HasValue
        {
            get
            {
                try
                {
                    return Value is not null;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region Properties

        [JsonIgnore] public string? MessageId { get; set; }

        /// <summary></summary>
        [JsonIgnore]
        public Type? ReturnedType { get; protected set; }

        /// <summary></summary>
        [JsonIgnore]
        public string? TracedPath { get; protected set; }

        /// <summary></summary>
        public bool Failed { get; }

        /// <summary></summary>
        [JsonIgnore]
        public string ResultId { get; protected set; }

        /// <summary></summary>
        [JsonIgnore]
        public string CreateAt { get; protected set; }

        /// <summary></summary>
        [JsonIgnore]
        public string? MessageType { get; protected set; }

        /// <summary></summary>
        [JsonProperty]
        public string? Code { get; protected set; }

        /// <summary></summary>
        [JsonProperty]
        public string? Description { get; protected set; }

        /// <summary></summary>
        [JsonProperty]
        public string? ResultStatus { get; protected set; }

        /// <summary></summary>
        [JsonProperty]
        public string? RedirectToUrl { get; protected set; }

        /// <summary></summary>
        [JsonProperty]
        public string? JsHandler { get; protected set; }

        /// <summary></summary>
        [JsonProperty]
        public string? RequestPath { get; protected set; }

        /// <summary></summary>
        [JsonIgnore]
        public string? TraceId { get; protected set; }

        /// <summary></summary>
        public int Status { get; protected set; }

        [JsonProperty] public IEnumerable<MessageModel>? Messages => MessageModels;

        [JsonProperty] public IEnumerable<ValidationFailure>? ValidationMessages => Failures;

        #endregion

        #region Methods

        [DebuggerStepThrough]
        public Result WithMessages(IEnumerable<MessageModel>? messages)
        {
            if (messages is null) return this;
            MessageModels = messages.ToList();
            return this;
        }

        [DebuggerStepThrough]
        public Result WithValidationMessages(params ValidationFailure[] failures)
        {
            Failures = failures.ToList();
            return this;
        }

        [DebuggerStepThrough]
        public Result WithValidationMessages(IEnumerable<ValidationFailure>? failures)
        {
            if (failures != null)
                Failures = failures.ToList();
            return this;
        }

        [DebuggerStepThrough]
        public Result AddMessage(string description, string code = "")
        {
            MessageModels?.Add(MessageModel.For(description, code));
            return this;
        }

        [DebuggerStepThrough]
        public Result AddMessage([NotNull] MessageModel message)
        {
            MessageModels?.Add(message);
            return this;
        }

        [DebuggerStepThrough]
        public Result AddValidationMessage([NotNull] ValidationFailure failure)
        {
            Failures?.Add(failure);
            return this;
        }

        public Result WithMessageId(string? value)
        {
            MessageId = value;
            return this;
        }


        public Result WithMessageType(string? value)
        {
            MessageType = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result WithRedirectUrl(string? value)
        {
            RedirectToUrl = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result WithJsHandler(string? value)
        {
            JsHandler = value;
            return this;
        }


        public Result WithRequestPath(string? value)
        {
            RequestPath = value;
            return this;
        }

        public Result WithTraceId(string? value)
        {
            TraceId = value;
            return this;
        }

        public Result WithStatus(int value)
        {
            Status = value;
            return this;
        }

        public Result WithReturnedType(Type? value)
        {
            ReturnedType = value;
            return this;
        }

        public Result WithTracedPath(string? value)
        {
            TracedPath = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result UpdateResultType(string? value)
        {
            ResultStatus = value;
            return this;
        }

        #endregion

        #region Helpers

        [DebuggerStepThrough]
        public static Result<T> OkAs<T>(T? value, string? description, string? code = "")
        {
            var type = typeof(Result).GetLastCalledType();
            var stackTraces = type.GetJsonStackTrace();
            return new Result<T>(value, false, description, code, null, null)
                .WithReturnedType(type)
                .WithTracedPath(stackTraces);
        }

        [DebuggerStepThrough]
        public static Result Ok(object? value, string? description, string? code = "")
        {
            var type = typeof(Result).GetLastCalledType();
            var stackTraces = type.GetJsonStackTrace();
            return new Result(value, false, description, code, null, null)
                .WithReturnedType(type)
                .WithTracedPath(stackTraces);
        }

        [DebuggerStepThrough]
        public static Result<T> OkAs<T>() => OkAs<T>(default, null, null);

        [DebuggerStepThrough]
        public static Result<T> OkAs<T>(T? value) => OkAs(value, null, null);

        [DebuggerStepThrough]
        public static Result<T> OkAs<T>(string? description, string? code = "")
            => OkAs<T>(default, description, code);

        [DebuggerStepThrough]
        public static Result<T> OkAs<T>(MessageModel messageModel)
            => OkAs<T>(default, messageModel.Description, messageModel.Code);

        [DebuggerStepThrough]
        public static Result<T> OkAs<T>(T value, MessageModel messageModel)
            => OkAs(value, messageModel.Description, messageModel.Code);

        [DebuggerStepThrough]
        public static Result Ok() => Ok(default, null, null);

        [DebuggerStepThrough]
        public static Result Ok(object? value) => Ok(value, null, null);

        [DebuggerStepThrough]
        public static Result Ok(string? description, string? code = "")
            => Ok(default, description, code);

        [DebuggerStepThrough]
        public static Result Ok(MessageModel messageModel)
            => Ok(default, messageModel.Description, messageModel.Code);

        [DebuggerStepThrough]
        public static Result Ok(object value, MessageModel messageModel)
            => Ok(value, messageModel.Description, messageModel.Code);


        [DebuggerStepThrough]
        public static Result<T> FailAs<T>(T? value, string? description, string? code = "")
        {
            var type = typeof(Result).GetLastCalledType();
            var stackTraces = type.GetJsonStackTrace();
            return new Result<T>(value, true, description, code, null, null).WithReturnedType(type)
                .WithTracedPath(stackTraces);
        }

        [DebuggerStepThrough]
        public static Result Fail(object? value, string? description, string? code = "")
        {
            var type = typeof(Result).GetLastCalledType();
            var stackTraces = type.GetJsonStackTrace();
            return new Result(value, true, description, code, null, null).WithReturnedType(type)
                .WithTracedPath(stackTraces);
        }

        [DebuggerStepThrough]
        public static Result<T> FailAs<T>(string? description, string? code = "")
            => FailAs<T>(default, description, code);

        [DebuggerStepThrough]
        public static Result<T> FailAs<T>(T value, [NotNull] MessageModel messageModel)
            => FailAs(value, messageModel.Description, messageModel.Code);

        [DebuggerStepThrough]
        public static Result<T> FailAs<T>([NotNull] MessageModel messageModel)
            => FailAs<T>(default, messageModel.Description, messageModel.Code);


        [DebuggerStepThrough]
        public static Result Fail(string? description, string? code = "")
            => Fail(default, description, code);

        [DebuggerStepThrough]
        public static Result Fail(object value, [NotNull] MessageModel messageModel)
            => Fail(value, messageModel.Description, messageModel.Code);

        [DebuggerStepThrough]
        public static Result Fail([NotNull] MessageModel messageModel)
            => Fail(default, messageModel.Description, messageModel.Code);

        [DebuggerStepThrough]
        public static Result<T> MapToFail<T>([NotNull] Result result, T? value = default)
        {
            return FailAs(value, result.Description, result.Code)
                .WithMessages(result.Messages)
                .WithValidationMessages(result.ValidationMessages)
                .WithTraceId(result.TraceId)
                .WithStatus(result.Status)
                .WithJsHandler(result.JsHandler)
                .WithRedirectUrl(result.RedirectToUrl)
                .WithRequestPath(result.RequestPath)
                .UpdateResultType(result.ResultStatus);
        }

        [DebuggerStepThrough]
        public static Result<T> MapToOk<T>([NotNull] Result result, T? value = default)
        {
            return OkAs(value, result.Description, result.Code)
                .WithMessages(result.Messages)
                .WithValidationMessages(result.ValidationMessages)
                .WithTraceId(result.TraceId)
                .WithStatus(result.Status)
                .WithJsHandler(result.JsHandler)
                .WithRedirectUrl(result.RedirectToUrl)
                .WithRequestPath(result.RequestPath)
                .UpdateResultType(result.ResultStatus);
        }


        [DebuggerStepThrough]
        public static Result Combine(params Result[] results)
        {
            if (results.All(x => !x.Failed))
            {
                var resultOk = results.Where(x => !x.Failed).Select(x=> x.Value);
                return Ok(resultOk);
            }

            var resultFailed = results.Where(x => x.Failed);
            return Fail(string.Join(Environment.NewLine, resultFailed.ToString()));
        }

        #endregion


        public static implicit operator string(Result value) => value.ToString();

        public override string ToString()
        {
            if (!Failed) return "Ok";

            StringBuilder builder = new();
            builder.Append("Result failed with: ");

            if (Code.IsNotEmpty())
                builder.Append($"{{Code}}: {Code}, ");

            if (Description.IsNotEmpty())
                builder.Append($"{{Description}}: {Description}, ");

            if (MessageType.IsNotEmpty())
                builder.Append($"{{MessageType}}: {MessageType}, ");

            if (MessageId.IsNotEmpty())
                builder.Append($"{{DomainId}}: {MessageId}, ");

            builder.Append($"{{Status}}: {Status}, ");

            if (RequestPath.IsNotEmpty())
                builder.Append($"{{RequestPath}}: {RequestPath}, ");


            if (RequestPath.IsNotEmpty())
                builder.Append($"{{RequestPath}}: {RequestPath}, ");


            if (TraceId.IsNotEmpty())
                builder.Append($"{{TraceId}}: {TraceId}, ");

            if (TracedPath.IsNotEmpty())
                builder.Append($"{{TracedPath}}: {TracedPath}, ");

            var messages = Messages?.Where(x => !x.IsNull()).ToList();
            if (messages?.Any() == true)
            {
                builder.Append("#ErrorMessages with: ");
                foreach (var message in messages)
                {
                    if (message.MessageId.IsNotEmpty())
                        builder.Append($"{nameof(MessageModel.MessageId)}: {message.MessageId}");

                    if (message.MessageType.IsNotEmpty())
                        builder.Append($"{nameof(MessageModel.MessageType)}: {message.MessageType}");

                    if (message.Code.IsNotEmpty())
                        builder.Append($"{nameof(MessageModel.Code)}: {message.Code}");

                    if (message.Description.IsNotEmpty())
                        builder.Append($"{nameof(MessageModel.Description)}: {message.Description}");

                    builder.Append($"{nameof(MessageModel.CreateAt)}: {message.CreateAt}");
                }
            }

            var validationMessages = ValidationMessages?.Where(x => !x.IsNull()).ToList();

            if (validationMessages?.Any() != true) return builder.ToString();

            builder.Append("#ValidationFailureMessages with: ");

            foreach (var message in validationMessages)
            {
                if (message.PropertyName.IsNotEmpty())
                    builder.Append($"{{PropertyName}}: {message.PropertyName}");

                if (message.ErrorMessage.IsNotEmpty())
                    builder.Append($"{{ErrorMessage}}: {message.ErrorMessage}");
            }


            return builder.ToString();
        }

        private void ReleaseUnmanagedResources()
        {
            MessageModels?.Clear();
            Failures?.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Result()
        {
            Dispose(false);
        }
    }

    /// <summary></summary>
    [Serializable]
    public class Result<T> : Result
    {
        private readonly T? _value;


        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <param name="failed"></param>
        /// <param name="description"></param>
        /// <param name="code"></param>
        /// <param name="messages"></param>
        /// <param name="failures"></param>
        [JsonConstructor]
        internal Result(T? value, bool failed, string? description, string? code,
            List<MessageModel>? messages,
            List<ValidationFailure>? failures)
            : base(value, failed, description, code, messages, failures)
        {
            _value = value;
        }

        public new T? Value => !Failed && _value != null ? _value : default;

        #region Methods

        [DebuggerStepThrough]
        public new Result<T> WithMessages(IEnumerable<MessageModel>? messages)
        {
            if (messages is null) return this;
            MessageModels = messages.ToList();
            return this;
        }

        [DebuggerStepThrough]
        public new Result<T> WithValidationMessages(params ValidationFailure[] failures)
        {
            Failures = failures.ToList();
            return this;
        }

        [DebuggerStepThrough]
        public new Result<T> WithValidationMessages(IEnumerable<ValidationFailure>? failures)
        {
            if (failures != null)
                Failures = failures.ToList();
            return this;
        }

        [DebuggerStepThrough]
        public new Result<T> AddMessage(string description, string code = "")
        {
            MessageModels?.Add(MessageModel.For(description, code));
            return this;
        }

        [DebuggerStepThrough]
        public new Result<T> AddMessage([NotNull] MessageModel message)
        {
            MessageModels?.Add(message);
            return this;
        }

        [DebuggerStepThrough]
        public new Result<T> AddValidationMessage([NotNull] ValidationFailure failure)
        {
            Failures?.Add(failure);
            return this;
        }

        public new Result<T> WithDomainId(string? value)
        {
            MessageId = value;
            return this;
        }


        public new Result<T> WithMessageType(string? value)
        {
            MessageType = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new Result<T> WithRedirectUrl(string? value)
        {
            RedirectToUrl = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new Result<T> WithJsHandler(string? value)
        {
            JsHandler = value;
            return this;
        }


        public new Result<T> WithRequestPath(string? value)
        {
            RequestPath = value;
            return this;
        }

        public new Result<T> WithTraceId(string? value)
        {
            TraceId = value;
            return this;
        }

        public new Result<T> WithStatus(int value)
        {
            Status = value;
            return this;
        }

        public new Result<T> WithReturnedType(Type? value)
        {
            ReturnedType = value;
            return this;
        }

        public new Result<T> WithTracedPath(string? value)
        {
            TracedPath = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new Result<T> UpdateResultType(string? value)
        {
            ResultStatus = value;
            return this;
        }

        #endregion
    }
}