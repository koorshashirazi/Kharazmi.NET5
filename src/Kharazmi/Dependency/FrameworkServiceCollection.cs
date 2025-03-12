using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Dependency
{
    /// <summary>
    /// An IServiceCollection implementation
    /// </summary>
    internal class FrameworkServiceCollection : IServiceCollection
    {
        private readonly IServiceCollection _services;

        /// <summary>
        /// Fired when a descriptor is added to the collection
        /// </summary>
        public event EventHandler<ServiceDescriptor>? ServiceAdded;

        /// <summary>
        /// Fired when a descriptor is removed to the collection
        /// </summary>
        public event EventHandler<ServiceDescriptor>? ServiceRemoved;

        /// <summary>
        /// Fired when a descriptor is cleared to the collection
        /// </summary>
        public event Action? ServicesClear;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameworkServiceCollection"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public FrameworkServiceCollection(IServiceCollection services)
            => _services = services;

        /// <summary>
        /// Get the value at index
        /// Setting is not supported
        /// </summary>
        public ServiceDescriptor this[int index]
        {
            get => _services[index];
            set => throw new NotSupportedException("Setting services in collection is not supported");
        }

        /// <summary>
        /// Count of services in the collection
        /// </summary>
        public int Count
        {
            get => _services.Count;
        }

        /// <summary>
        /// Obviously not
        /// </summary>
        public bool IsReadOnly
        {
            get => false;
        }

        /// <summary>
        /// Index of item in the list
        /// </summary>
        public int IndexOf(ServiceDescriptor item) => _services.IndexOf(item);


        /// <summary>
        /// True is the item exists in the collection
        /// </summary>
        public bool Contains(ServiceDescriptor item) => _services.Contains(item);

        /// <summary>
        /// Adding a service descriptor will fire the ServiceAdded event
        /// </summary>
        /// <param name="item"></param>
        public void Add(ServiceDescriptor item)
        {
            _services.Add(item);
            ServiceAdded?.Invoke(this, item);
        }

        /// <summary>
        /// Inserting no supported
        /// </summary>
        public void Insert(int index, ServiceDescriptor item) =>
            throw new NotSupportedException("Inserting services in collection is not supported");

        /// <summary>
        /// Removing items
        /// </summary>
        public bool Remove(ServiceDescriptor item)
        {
            var result = _services.Remove(item);
            ServiceRemoved?.Invoke(this, item);
            return result;
        }

        /// <summary>
        /// Removing items not supported
        /// </summary>
        public void RemoveAt(int index) =>
            throw new NotSupportedException("Removed services in collection is not supported");


        /// <summary>
        /// Copy items to array of service descriptors
        /// </summary>
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _services.CopyTo(array, arrayIndex);

        /// <summary>
        /// Clear the collection
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            ServicesClear?.Invoke();
        }

        /// <summary>
        /// Enumerator for objects
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _services).GetEnumerator();

        /// <summary>
        /// Enumerator for service descriptors
        /// </summary>
        public IEnumerator<ServiceDescriptor> GetEnumerator() => _services.GetEnumerator();
    }
}