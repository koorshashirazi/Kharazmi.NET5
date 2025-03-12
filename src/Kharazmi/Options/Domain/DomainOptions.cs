using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kharazmi.Constants;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Handlers;
using Kharazmi.Models;
using Kharazmi.Pipelines;

namespace Kharazmi.Options.Domain
{
    public class DomainOption : ConfigurePluginOption
    {
        private bool _useDispatchers;
        private bool _useEventProcessor;
        private bool _useDomainValidation;
        private bool _useAutoRegisterHandler;
        private bool _useDomainMetadata;
        private DomainMetadataOption _domainMetadataOption;
        private List<StringBoolean> _commandPipelines;
        private List<StringBoolean> _eventPipelines;
        private List<StringBoolean> _queryPipelines;
        private static readonly List<TypeBoolean> Commands;
        private static readonly List<TypeBoolean> Events;
        private static readonly List<TypeBoolean> Queries;

        static DomainOption()
        {
            Commands = new();
            Events = new();
            Queries = new();
        }

        public DomainOption()
        {
            Enable = true;
            RetryOption = new RetryOption();
            _domainMetadataOption = new DomainMetadataOption();
            DispatchAsynchronous = false;
            AssemblyHandler = "";
            AssemblyDomainValidation = "";
            _commandPipelines = new List<StringBoolean>();
            _eventPipelines = new List<StringBoolean>();
            _queryPipelines = new List<StringBoolean>();
        }

        public bool UseDispatchers
        {
            get => Enable && _useDispatchers;
            set => _useDispatchers = value;
        }

        public bool DispatchAsynchronous { get; set; }

        public bool UseEventProcessor
        {
            get => Enable && _useEventProcessor;
            set => _useEventProcessor = value;
        }

        public bool UseAutoRegisterHandler
        {
            get => Enable && _useAutoRegisterHandler;
            set => _useAutoRegisterHandler = value;
        }

        public string AssemblyHandler { get; set; }

        public bool UseDomainValidation
        {
            get => Enable && _useDomainValidation;
            set => _useDomainValidation = value;
        }

        public string AssemblyDomainValidation { get; set; }

        public bool UseDomainMetadata
        {
            get => Enable && _useDomainMetadata;
            set => _useDomainMetadata = value;
        }

        public DomainMetadataOption DomainMetadataOption
        {
            get => _domainMetadataOption;
            set
            {
                if (value.IsNull()) return;
                _domainMetadataOption = value;
            }
        }

        public RetryOption RetryOption { get; set; }

        public List<StringBoolean> CommandPipelines
        {
            get => _commandPipelines;
            set => _commandPipelines = new HashSet<StringBoolean>(value).ToList();
        }

        public List<StringBoolean> EventPipelines
        {
            get => _eventPipelines;
            set => _eventPipelines = new HashSet<StringBoolean>(value).ToList();
        }

        public List<StringBoolean> QueryPipelines
        {
            get => _queryPipelines;
            set => _queryPipelines = new HashSet<StringBoolean>(value).ToList();
        }


        public IEnumerable<Type?> EnabledCommandPipelines()
        {
            foreach (var pipeline in CommandPipelines.Where(x => x.Value).Reverse())
            {
                var command = Commands.FirstOrDefault(x => GetPipelineName(x.Key) == pipeline.Key);
                yield return command?.Key;
            }
        }

        public IEnumerable<Type?> EnabledEventPipelines()
        {
            foreach (var pipeline in EventPipelines.Where(x => x.Value).Reverse())
                yield return Events.FirstOrDefault(x => GetPipelineName(x.Key) == pipeline.Key)?.Key;
        }

        public IEnumerable<Type?> EnabledQueryPipelines()
        {
            foreach (var pipeline in QueryPipelines.Where(x => x.Value).Reverse())
                yield return Queries.FirstOrDefault(x => GetPipelineName(x.Key) == pipeline.Key)?.Key;
        }

        public void AddOrUpdateCommandPipeline(Type pipelineType, bool enable)
        {
            if (typeof(ICommandPipeline).IsAssignableFrom(pipelineType) == false)
                throw new PipelineException(
                    $"Type of {pipelineType.Name} is not assignable from {nameof(ICommandPipeline)}");

            var attribute = pipelineType.GetCustomAttribute<PipelineAttribute>();

            if (attribute.IsNull())
                throw new PipelineException(
                    $"Type of {pipelineType.Name} must have a attribute with type {nameof(PipelineAttribute)}");

            if (attribute.PipelineType != PipelineType.DomainCommand)
                throw new PipelineException(
                    $"Pipeline type of type of {pipelineType.Name} must be {PipelineType.DomainCommand}");

            var pipelineName = GetPipelineName(pipelineType);
            var pipeline = _commandPipelines.FirstOrDefault(x => x.Key == pipelineName);
            if (pipeline is null)
                _commandPipelines.Add(new StringBoolean(pipelineName, enable));

            var command = Commands.FirstOrDefault(x => x.Key == pipelineType);
            if (command is null)
            {
                Commands.Add(pipeline is null
                    ? new TypeBoolean(pipelineType, enable)
                    : new TypeBoolean(pipelineType, pipeline.Value));
                return;
            }

            var indexOfCommand = Commands.IndexOf(command);
            Commands[indexOfCommand] = pipeline is null
                ? new TypeBoolean(pipelineType, enable)
                : new TypeBoolean(pipelineType, pipeline.Value);
        }

        public void AddDomainEventPipeline(Type pipelineType, bool enable)
        {
            if (typeof(IEventPipeline).IsAssignableFrom(pipelineType) == false)
                throw new PipelineException(
                    $"Type of {pipelineType.Name} is not assignable from {nameof(IEventPipeline)}");

            var attribute = pipelineType.GetCustomAttribute<PipelineAttribute>();

            if (attribute.IsNull())
                throw new PipelineException(
                    $"Type of {pipelineType.Name} must have a attribute with type {nameof(PipelineAttribute)}");

            if (attribute.PipelineType != PipelineType.DomainEvent)
                throw new PipelineException(
                    $"Pipeline type of type of {pipelineType.Name} must be {PipelineType.DomainEvent}");

            var pipelineName = GetPipelineName(pipelineType);
            var pipeline = _eventPipelines.FirstOrDefault(x => x.Key == pipelineName);
            if (pipeline is null)
                _eventPipelines.Add(new StringBoolean(pipelineName, enable));

            var events = Events.FirstOrDefault(x => x.Key == pipelineType);
            if (events is null)
            {
                Events.Add(pipeline is null
                    ? new TypeBoolean(pipelineType, enable)
                    : new TypeBoolean(pipelineType, pipeline.Value));
                return;
            }

            var indexOfCommand = Events.IndexOf(events);
            Events[indexOfCommand] = pipeline is null
                ? new TypeBoolean(pipelineType, enable)
                : new TypeBoolean(pipelineType, pipeline.Value);
        }

        public void AddDomainQueryPipeline(Type pipelineType, bool enable)
        {
            if (typeof(IQueryPipeline).IsAssignableFrom(pipelineType) == false)
                throw new PipelineException(
                    $"Type of {pipelineType.Name} is not assignable from {nameof(IQueryPipeline)}");

            var attribute = pipelineType.GetCustomAttribute<PipelineAttribute>();

            if (attribute.IsNull())
                throw new PipelineException(
                    $"Type of {pipelineType.Name} must have a attribute with type {nameof(PipelineAttribute)}");

            if (attribute.PipelineType != PipelineType.DomainQuery)
                throw new PipelineException(
                    $"Pipeline type of type of {pipelineType.Name} must be {PipelineType.DomainQuery}");

            var pipelineName = GetPipelineName(pipelineType);
            var pipeline = _queryPipelines.FirstOrDefault(x => x.Key == pipelineName);
            if (pipeline is null)
                _queryPipelines.Add(new StringBoolean(pipelineName, enable));

            var queries = Queries.FirstOrDefault(x => x.Key == pipelineType);
            if (queries is null)
            {
                Queries.Add(pipeline is null
                    ? new TypeBoolean(pipelineType, enable)
                    : new TypeBoolean(pipelineType, pipeline.Value));
                return;
            }

            var indexOfCommand = Queries.IndexOf(queries);
            Queries[indexOfCommand] = pipeline is null
                ? new TypeBoolean(pipelineType, enable)
                : new TypeBoolean(pipelineType, pipeline.Value);
        }


        public void RemoveDomainCommandPipeline(Type pipeline)
        {
            if (typeof(ICommandHandler<>).IsAssignableFrom(pipeline) == false)
                throw new InvalidCastException($"Type of {pipeline.Name} is not assignable from ICommandHandler");

            Commands.RemoveAll(x => x.Key == pipeline);
        }

        public void RemoveDomainEventPipeline(Type pipeline)
        {
            if (typeof(IEventHandler<>).IsAssignableFrom(pipeline) == false)
                throw new InvalidCastException($"Type of {pipeline.Name} is not assignable from IDomainEventHandler");

            Events.RemoveAll(x => x.Key == pipeline);
        }

        public void RemoveDomainQueryPipeline(Type pipeline)
        {
            if (typeof(IQueryHandler<,>).IsAssignableFrom(pipeline) == false)
                throw new InvalidCastException($"Type of {pipeline.Name} is not assignable from IQueryHandler");

            Queries.RemoveAll(x => x.Key == pipeline);
        }

        public void RemoveCommandPipelines() => Commands.Clear();
        public void RemoveEventPipelines() => Events.Clear();
        public void RemoveQueryPipelines() => Queries.Clear();

        public override void Validate()
        {
            if (UseAutoRegisterHandler && AssemblyHandler.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(DomainOption), nameof(AssemblyHandler)));

            if (UseDomainValidation && AssemblyDomainValidation.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(DomainOption), nameof(AssemblyDomainValidation)));
            if (_useDomainMetadata)
            {
                DomainMetadataOption.Validate();
                AddValidations(DomainMetadataOption.ValidationResults());
            }

            RetryOption.Validate();
            AddValidations(RetryOption.ValidationResults());
        }

        private static string GetPipelineName(Type pipelineType)
        {
            var att = pipelineType.GetCustomAttribute<PipelineAttribute>();
            var typeName = pipelineType.GetGenericTypeName(false);
            var name = att?.Name ?? typeName;
            return name;
        }
    }
}