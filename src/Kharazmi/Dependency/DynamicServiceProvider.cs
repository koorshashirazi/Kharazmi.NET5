using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Dependency
{
    internal class ServiceDescriptorEquality : IEqualityComparer<ServiceDescriptor>
    {
        public bool Equals(ServiceDescriptor? x, ServiceDescriptor? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Lifetime == y.Lifetime && x.ServiceType == y.ServiceType;
        }

        public int GetHashCode(ServiceDescriptor obj)
        {
            return HashCode.Combine((int) obj.Lifetime, obj.ServiceType);
        }
    }

    /// <summary>
    /// Service provider that allows for dynamic adding of new services
    /// </summary>
    public class DynamicServiceProvider : IDynamicServiceProvider, IDisposable
    {
        private readonly HashSet<ServiceProvider> _serviceProviders;
        private readonly FrameworkServiceCollection _services;
        private readonly object _servicesLock = new();
        private HashSet<ServiceDescriptor> _serviceDescriptors;
        private readonly Dictionary<Type, object> _cachedServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicServiceProvider"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public DynamicServiceProvider(IServiceCollection services)
        {
            _serviceProviders = new HashSet<ServiceProvider>();
            _serviceDescriptors = new HashSet<ServiceDescriptor>(new ServiceDescriptorEquality());
            _cachedServices = new Dictionary<Type, object>();
            _services = new FrameworkServiceCollection(services);

            services.AddSingleton<IDynamicServiceProvider>(this);
            _serviceProviders.Add(services.BuildServiceProvider(true));

            _services.ServiceAdded += OnServiceAdded;
            _services.ServiceRemoved += OnServiceRemoved;
            _services.ServicesClear += OnServicesClear;
        }


        /// <summary>
        /// Add services to this collection
        /// </summary>
        public IServiceCollection ServiceCollection => _services;

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType. -or- null if there is no service object of type serviceType.</returns>
        public object? GetService(Type serviceType)
        {
            lock (_servicesLock)
            {
                var service = GetServiceInternal(serviceType);

                if (service is null && _serviceDescriptors.Count > 0)
                {
                    // Add a new service collection in provider in the chain
                    var newCollection = new ServiceCollection();
                    foreach (var descriptor in _services)
                    foreach (var descriptorToAdd in BuildServiceDescriptors(descriptor))
                        ((ICollection<ServiceDescriptor>) newCollection).Add(descriptorToAdd);

                    var newServiceProvider = newCollection.BuildServiceProvider(true);
                    _serviceProviders.Add(newServiceProvider);
                    _serviceDescriptors = new HashSet<ServiceDescriptor>();
                    service = newServiceProvider.GetService(serviceType);
                }

                if (service != null)
                    _cachedServices[serviceType] = service;

                return service;
            }
        }

        private IEnumerable<ServiceDescriptor> BuildServiceDescriptors(ServiceDescriptor descriptor)
        {
            if (_serviceDescriptors.TryGetValue(descriptor, out var sd))
            {
                yield return sd;
                yield break;
            }

            if (!descriptor.ServiceType.IsGenericTypeDefinition)
            {
                var service = GetServiceInternal(descriptor.ServiceType);
                if (service is null)
                {
                    yield return descriptor;
                    yield break;
                }

                yield return ServiceDescriptor.Describe(descriptor.ServiceType, _ => service, descriptor.Lifetime);
                yield break;
            }


            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                var genericsSingleton = _cachedServices.Keys.Where(t =>
                    t.IsGenericType && t.GetGenericTypeDefinition() == descriptor.ServiceType);

                foreach (var servType in genericsSingleton)
                {
                    yield return ServiceDescriptor.Describe(servType, _ => _cachedServices[servType],
                        ServiceLifetime.Singleton);
                }
            }

            yield return descriptor;
        }

        private object? GetServiceInternal(Type serviceType)
        {
            foreach (var serviceProvider in _serviceProviders)
            {
                var service = serviceProvider.GetService(serviceType);

                if (service == null) continue;

                if (service is not IEnumerable enumerable) return service;

                return !enumerable.GetEnumerator().MoveNext() ? null : service;
            }

            return null;
        }

        private void OnServicesClear()
        {
            try
            {
                Monitor.Enter(_servicesLock);
                _serviceDescriptors.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Monitor.Exit(_servicesLock);
            }
        }

        private void OnServiceRemoved(object? sender, ServiceDescriptor item)
        {
            try
            {
                Monitor.Enter(_servicesLock);
                _serviceDescriptors.Remove(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Monitor.Exit(_servicesLock);
            }
        }

        private void OnServiceAdded(object sender, ServiceDescriptor item)
        {
            try
            {
                Monitor.Enter(_servicesLock);
                _serviceDescriptors.TryGetValue(item, out var actualValue);
                if (actualValue is null)
                    _serviceDescriptors.Add(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Monitor.Exit(_servicesLock);
            }
        }

        /// <summary>
        /// Dispose the provider and all resolved services
        /// </summary>
        public void Dispose()
        {
            lock (_servicesLock)
            {
                _services.ServiceAdded -= OnServiceAdded;
                _services.ServiceRemoved -= OnServiceRemoved;
                _services.ServicesClear -= OnServicesClear;
                foreach (var serviceProvider in _serviceProviders)
                {
                    try
                    {
                        serviceProvider.Dispose();
                    }
                    catch
                    {
                        // singleton classes might be disposed twice and throw some exception
                    }
                }

                _serviceDescriptors.Clear();
                _cachedServices.Clear();
                _serviceProviders.Clear();
            }
        }
    }
}