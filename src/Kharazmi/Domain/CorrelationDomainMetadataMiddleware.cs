using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Hooks;
using Kharazmi.Options.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Domain
{
    internal class CorrelationDomainMetadataMiddleware : IMiddleware
    {
        private readonly ISettingProvider _settings;
        private readonly IDomainMetadataFactory _domainMetadataFactory;
        private readonly IEnumerable<IDomainMetadataHook> _domainMetadataHooks;
        private readonly ILogger _logger;

        public CorrelationDomainMetadataMiddleware(
            ILogger<CorrelationDomainMetadataMiddleware>? logger,
            ISettingProvider settings,
            IDomainMetadataFactory domainMetadataFactory,
            ServiceFactory<IDomainMetadataHook> domainMetadataHook)
        {
            _settings = settings;
            _domainMetadataFactory = domainMetadataFactory;
            _domainMetadataHooks = domainMetadataHook.Instances();
            _logger = logger ?? NullLoggerFactory.Instance.CreateLogger(nameof(CorrelationDomainMetadataMiddleware));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var domainOption = _settings.Get<DomainOption>();
                if (domainOption.Enable == false || domainOption.UseDomainMetadata == false)
                {
                    await next(context);
                    return;
                }

                _logger.LogTrace(MessageTemplate.DomainMetadataProcessingBegin, MessageEventName.DomainMetadata,
                    nameof(CorrelationDomainMetadataMiddleware),
                    context.Request.Path.HasValue ? context.Request.Path.Value : "");

                var metadataOptions = domainOption.DomainMetadataOption;
                var metadataIdOptions = metadataOptions.DomainIdMetadataOption;
                var domainMetadata = _domainMetadataHooks.GetDomainMetadata();

                _domainMetadataFactory.CreateFrom(domainMetadata);

                if (metadataOptions.UseDomainId == false)
                {
                    await next(context);
                    return;
                }


                var domainId = GetDomainIdFromHeader(context.Request.Headers, metadataIdOptions);

                if (domainId.IsEmpty() && metadataIdOptions.EnsureExistInHeader)
                {
                    _logger.LogTrace(MessageTemplate.DomainMetadataCanNotBuildDomainId, MessageEventName.DomainMetadata,
                        nameof(CorrelationDomainMetadataMiddleware));

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync($"Invalid ${metadataIdOptions.RequestHeader} header request");
                    return;
                }

                if (domainId.IsEmpty())
                    domainId = BuildDomainId(metadataIdOptions, domainMetadata);

                if (domainId.IsEmpty())
                {
                    await next(context);
                    return;
                }

                _domainMetadataFactory.UpdateCurrent(x => { x.SetDomainId(DomainId.FromString(domainId)); });

                ReplaceWithTraceId(context, domainId, metadataIdOptions);

                _logger.LogTrace(MessageTemplate.DomainMetadataCreating, MessageEventName.DomainMetadata,
                    nameof(CorrelationDomainMetadataMiddleware));

                WriteToResponse(context, domainId, metadataIdOptions);

                WriteToLoggingScope(domainId, metadataIdOptions);

                await next(context);

                _logger.LogTrace(MessageTemplate.DomainMetadataDisposing, MessageEventName.DomainMetadata,
                    nameof(CorrelationDomainMetadataMiddleware));

                _domainMetadataFactory.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogTrace("{Message}", e.Message);
                await next(context);
            }
        }


        private void ReplaceWithTraceId(HttpContext context, string domainId, DomainIdMetadataOption option)
        {
            if (!option.ReplaceWithTraceId) return;

            _logger.LogTrace(MessageTemplate.DomainMetadataUpdatingTraceId, MessageEventName.DomainMetadata,
                nameof(CorrelationDomainMetadataMiddleware));

            context.TraceIdentifier = domainId;
            _domainMetadataFactory.UpdateCurrent(x => x.SetTraceId(domainId));
        }

        private void WriteToLoggingScope(string domainId, DomainIdMetadataOption domainIdMetadataOption)
        {
            if (domainIdMetadataOption.WriteToLoggingScope)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    [domainIdMetadataOption.LoggingScopeKey] = domainId
                }))
                {
                    _logger.LogTrace(MessageTemplate.DomainMetadataProcessingEnd, MessageEventName.DomainMetadata,
                        nameof(CorrelationDomainMetadataMiddleware), domainId);
                }
            }

            _logger.LogTrace(MessageTemplate.DomainMetadataProcessingEnd, MessageEventName.DomainMetadata,
                nameof(CorrelationDomainMetadataMiddleware), domainId);
        }

        private void WriteToResponse(HttpContext context, string domainId, DomainIdMetadataOption option)
        {
            if (option.WriteToResponse)
            {
                context.Response.OnStarting(() =>
                {
                    if (context.Response.Headers.ContainsKey(option.ResponseHeader))
                        return Task.CompletedTask;

                    _logger.LogTrace(MessageTemplate.DomainMetadataWritingDomainIdToResponseHeader,
                        MessageEventName.DomainMetadata,
                        nameof(CorrelationDomainMetadataMiddleware), option.ResponseHeader, domainId);

                    context.Response.Headers.Add(option.ResponseHeader, domainId);

                    return Task.CompletedTask;
                });
            }
        }

        private string BuildDomainId(DomainIdMetadataOption option, DomainMetadata domainMetadata)
        {
            _logger.LogTrace(MessageTemplate.DomainMetadataNotFoundDomainIdHeader, MessageEventName.DomainMetadata,
                nameof(CorrelationDomainMetadataMiddleware));

            if (option.DomainIdGenerator is null) return domainMetadata.DomainId;

            var domainId = option.DomainIdGenerator();

            _logger.LogTrace(MessageTemplate.DomainMetadataGeneratedDomainIdUsingFunction,
                MessageEventName.DomainMetadata, nameof(CorrelationDomainMetadataMiddleware), domainId);

            return domainId;
        }

        private string? GetDomainIdFromHeader(IHeaderDictionary headerDictionary,
            DomainIdMetadataOption metadataIdOptions)
        {
            headerDictionary.TryGetValue(metadataIdOptions.RequestHeader, out var stringValues);

            var domainId = stringValues.FirstOrDefault();

            if (domainId.IsNotEmpty()) return domainId;
            
            _logger.LogTrace(MessageTemplate.DomainMetadataMissingDomainIdHeader, MessageEventName.DomainMetadata,
                nameof(CorrelationDomainMetadataMiddleware));
            
            return null;

        }
    }
}