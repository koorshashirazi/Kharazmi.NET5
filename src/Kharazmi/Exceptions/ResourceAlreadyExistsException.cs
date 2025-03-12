#region

using System;

#endregion

namespace Kharazmi.Exceptions
{
    public class ResourceAlreadyExistsException : AppException
    {
        public Guid Id { get; }

        public ResourceAlreadyExistsException(Guid id) : base($"Resource with id: {id} already exists.")
        {
            Id = id;
            Code = "resource_already_exists";
        }
    }
}