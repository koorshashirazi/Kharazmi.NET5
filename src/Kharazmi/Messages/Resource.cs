﻿namespace Kharazmi.Messages
{
    public class Resource
    {
        public string? Service { get; }
        public string? Endpoint { get; }

        protected Resource()
        {
        }

        protected Resource(string service, string endpoint)
        {
            Service = service.ToLowerInvariant();
            Endpoint = endpoint.ToLowerInvariant();
        }

        public static Resource Create(string service, string endpoint)
        {
            return new(service, endpoint);
        }
    }
}