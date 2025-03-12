using System;
using System.Collections.Generic;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Pipelines;

namespace Kharazmi.Constants
{
    public static class MessageHelper
    {
        public static string HealthCheckResult(string eventName, string categoryName, string validationResult)
            => MessageTemplate.HealthCheckResult
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", categoryName)
                .Replace("{ValidationResult}", validationResult);

        public static string HealthCheckReport(string eventName, string categoryName, string status)
            => MessageTemplate.HealthCheckReport
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", categoryName)
                .Replace("{Status}", status);

        public static string CanNotLoadAssemblyFromType(string eventName, string messageName, string assemblyType)
            => MessageTemplate.AssemblyTypeLoad
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{AssemblyType}", assemblyType);

        public static string CanNotUseOpenId(string messageName) =>
            MessageTemplate.CanNotUseOpenIdConnect
                .Replace("{EventName}", MessageEventName.OpenIdConnect)
                .Replace("{CategoryName}", messageName);

        public static string CanNotUseHangfire(string messageName) =>
            MessageTemplate.CanNotUseHangfire
                .Replace("{EventName}", MessageEventName.Hangfire)
                .Replace("{CategoryName}", messageName);

        public static string CanNotUseHangfireStorage(string messageName, string storageType) =>
            MessageTemplate.CanNotUseHangfireStorage
                .Replace("{EventName}", MessageEventName.Hangfire)
                .Replace("{CategoryName}", messageName)
                .Replace("{StorageType}", storageType);

        public static string MaxAllowed(string eventName, string messageName, string valueName,
            TimeSpan value, TimeSpan? maxValue = default) =>
            MessageTemplate.MaxAllowedFormat
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{ValueName}", valueName)
                .Replace("{MaximumValue}",
                    maxValue.HasValue
                        ? maxValue.Value.ToStringFormat()
                        : TimeSpan.FromMilliseconds(int.MaxValue).ToStringFormat())
                .Replace("{GivenValue}", value.ToStringFormat());

        public static string MustBeGreaterThan(string eventName, string messageName, string valueName,
            TimeSpan value, TimeSpan? maxValue = default) =>
            MessageTemplate.MustBeGreaterThanFormat
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{ValueName}", valueName)
                .Replace("{MaxValue}",
                    maxValue.HasValue
                        ? maxValue.Value.ToStringFormat()
                        : TimeSpan.FromMilliseconds(int.MaxValue).ToStringFormat())
                .Replace("{GivenValue}", value.ToStringFormat());
        
        public static string MustBePositive(string eventName, string messageName, string valueName,
            TimeSpan value) =>
            MessageTemplate.MustBePositive
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{ValueName}", valueName)
                .Replace("{GivenValue}", value.ToStringFormat());

        public static string MustBeGreaterThan(string eventName, string messageName, string valueName,
            long currentValue, long maxValue) =>
            MessageTemplate.MustBeGreaterThanFormat
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{ValueName}", valueName)
                .Replace("{MaxValue}", $"{maxValue}")
                .Replace("{GivenValue}", $"{currentValue}");

        public static string MustBeBetween(string eventName, string messageName, string valueName,
            TimeSpan currentValue, TimeSpan? minValue = default, TimeSpan? maxValue = default) =>
            MessageTemplate.MustBeBetweenFormat
                .Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{ValueName}", valueName)
                .Replace("{MinValue}",
                    minValue.HasValue
                        ? minValue.Value.ToStringFormat()
                        : TimeSpan.Zero.ToStringFormat())
                .Replace("{MaxValue}", maxValue.HasValue
                    ? maxValue.Value.ToStringFormat()
                    : TimeSpan.FromMilliseconds(int.MaxValue).ToStringFormat())
                .Replace("{GivenValue}", currentValue.ToStringFormat());

        public static string NullOrEmpty(string eventName, string messageName, string name)
            => MessageTemplate.NullOrEmpty.Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{Name}", name);

        public static string NotFoundValueInCollection(string eventName, string messageName, string collectionName,
            string keyName)
            => MessageTemplate.NotFoundValueInCollection.Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{CollectionName}", collectionName)
                .Replace("{KeyName}", keyName);

        public static string OutOfRange(string eventName, string messageName, IEnumerable<string> valuesRange,
            string keyName)
            => MessageTemplate.OutOfRange.Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{ValuesRange}", string.Join("," , valuesRange))
                .Replace("{KeyName}", keyName);
        public static string HangfireInvalidStorage(string eventName, string messageName)
            => MessageTemplate.HangfireInvalidStorage.Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{StorageTypes}", string.Join(',', typeof(JobStorageTypes).GetConstants()));

        public static string OptionValidation(string eventName, string messageName, string optionType,
            IEnumerable<string?> results)
            => MessageTemplate.OptionValidation.Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{OptionType}", optionType)
                .Replace("{ValidationResults}",
                    results.IsNull() ? "" : string.Join(',', results));

        public static string NotFoundFilePath(string eventName, string messageName, string filePath)
            => MessageTemplate.NotFoundFilePath.Replace("{EventName}", eventName)
                .Replace("{CategoryName}", messageName)
                .Replace("{FilePath}", filePath);      
        
        public static string BeforeMessageProcessing(PipelineEventLog eventLog)
        {
            return MessageTemplate.BeforeMessageProcessing.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{MessageType}", eventLog.MessageType.GetGenericTypeName())
                .Replace("{DomainMetadata}", eventLog.DomainMetadata.ToString())
                .Replace("{Value}", eventLog.Message.ToMetadata().ToString());
        }
        
        public static string MessageProcessingFailed(PipelineEventLog eventLog, Result result)
        {
            return MessageTemplate.MessageProcessingFailed.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{MessageType}", eventLog.MessageType.GetGenericTypeName())
                .Replace("{ResultDescription}", result.Description);
        }
        public static string MessageProcessSucceeded(PipelineEventLog eventLog)
        {
            return MessageTemplate.MessageProcessSucceeded.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{MessageType}", eventLog.MessageType.GetGenericTypeName());
        }
        
        public static string MessageProcessException(PipelineEventLog eventLog, Exception ex)
        {
            return MessageTemplate.MessageProcessException.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{MessageType}", eventLog.MessageType.GetGenericTypeName())
                .Replace("{ExceptionMessage}", ex.Message);
        }
        
        public static string DomainRetryBeforeHandleMessage(RetryEventLog eventLog)
        {
            return MessageTemplate.DomainRetryBeforeHandleMessage.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{DomainMetadata}", eventLog.DomainMetadata.ToString());
        }
        
        public static string DomainRetryAfterHandleMessage(RetryEventLog eventLog)
        {
            return MessageTemplate.DomainRetryAfterHandleMessage.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{DomainMetadata}", eventLog.DomainMetadata.ToString())
                .Replace("{TotalSeconds}", $"{eventLog.TotalSeconds:F}");
        }
        
        public static string DomainRetryException(RetryEventLog eventLog, Exception ex)
        {
            return MessageTemplate.DomainRetryException.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{DomainMetadata}", eventLog.DomainMetadata.ToString())
                .Replace("{ExceptionMessage}", ex.Message);
        }
        
        public static string DomainRetry(RetryEventLog eventLog, Exception ex)
        {
            return MessageTemplate.DomainRetry.Replace("{EventName}", eventLog.EventName)
                .Replace("{CategoryName}", eventLog.CategoryName)
                .Replace("{DomainMetadata}", eventLog.DomainMetadata.ToString())
                .Replace("{Attempt}", $"{eventLog.Attempt}")
                .Replace("{ExceptionType}", ex.GetType().Name)
                .Replace("{ExceptionMessage}", ex.Message);
        }
        
    }
}