using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Exceptions;
using Kharazmi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Dependency
{
    public class ServiceFactory<T> where T : class
    {
        private readonly Func<IEnumerable<T>> _activator;
        private readonly ILogger<ServiceFactory<T>>? _logger;

        public ServiceFactory(
            Func<IEnumerable<T>> activator,
            ISettingProvider settings,
            ILoggerFactory? loggerFactory = null)
        {
            Settings = settings;
            LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _activator = activator;
            _logger = LoggerFactory.CreateLogger<ServiceFactory<T>>();
        }

        public ISettingProvider Settings { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IEnumerable<T> Instances() => _activator();

        public T Instance(Func<T, bool>? predicate = null)
        {
            var instances = predicate is null ? _activator().ToArray() : _activator().Where(predicate).ToArray();

            try
            {
                var instance = instances.Length == 1
                    ? instances.First()
                    : instances.First(x => !x.GetType().IsAssignableTo(typeof(INullInstance)));

                return instance;
            }
            catch (Exception e)
            {
                _logger?.LogWarning(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceResolver,
                    nameof(ServiceFactory<T>), instances.GetType().Name, typeof(T).Name, e.Message);
                try
                {
                    return instances.First(x => x.GetType().IsAssignableTo(typeof(INullInstance)));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceResolver,
                        nameof(ServiceFactory<T>), instances.GetType().Name, typeof(T).Name, ex.Message);
                    throw new ServiceResolverException(typeof(T), instances.GetType());
                }
            }
        }

        public T InstanceBy(string instanceName)
        {
            var instances = _activator().ToList();

            try
            {
                if (instances.Count == 1) return instances.First();

                return instances.First(x =>
                    string.Equals(x.GetType().Name, instanceName) &&
                    !x.GetType().IsAssignableTo(typeof(INullInstance)));
            }
            catch (Exception e)
            {
                _logger?.LogWarning(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceResolver,
                    nameof(ServiceFactory<T>), typeof(T).Name, e.Message);

                try
                {
                    return instances.First(x => x.GetType().IsAssignableTo(typeof(INullInstance)));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceResolver,
                        nameof(ServiceFactory<T>), typeof(T).Name, ex.Message);
                    throw new ServiceResolverException(typeof(T), instances.GetType());
                }
            }
        }

        public T InstanceBy(RuleOption ruleOption)
        {
            var instances = _activator().ToList();

            try
            {
                if (instances.Count == 1) return instances.First();

                return instances.First(x =>
                    x.GetType() == ruleOption.ImplementationType &&
                    !x.GetType().IsAssignableTo(typeof(INullInstance)));
            }
            catch (Exception e)
            {
                _logger?.LogWarning(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceResolver,
                    nameof(ServiceFactory<T>), typeof(T).Name, e.Message);

                try
                {
                    return instances.First(x => x.GetType().IsAssignableTo(typeof(INullInstance)));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceResolver,
                        nameof(ServiceFactory<T>), typeof(T).Name, ex.Message);
                    throw new ServiceResolverException(typeof(T), instances.GetType());
                }
            }
        }
    }
}