using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Kharazmi.Configuration;
using Kharazmi.Conventions;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Extensions;

namespace Kharazmi.RabbitMq
{
    
    internal class MessageConventionFactory : IMessageConventionFactory, IDisposable
    {
        private readonly ISettingProvider _settingProvider;
        private readonly ConcurrentDictionary<Type, IMessageConventions> _conventions = new();

        public MessageConventionFactory(ISettingProvider settingProvider)
           => _settingProvider = settingProvider;

        public IReadOnlyDictionary<Type, IMessageConventions> GetAll()
            => _conventions;
        
        public IMessageConventions TryGetOrAdd(Type messageType)
            => _conventions.GetOrAdd(messageType, Create);

        private IMessageConventions Create(Type messageType)
        {
            var option = _settingProvider.Get<RabbitMqOption>();
            var exchangeNaming = messageType.GetExchangeName(option);
            var routingKey = messageType.GetRoutingKey(option);
            var queueNaming = messageType.GetQueueName(option);
            return new MessageConventions(exchangeNaming, routingKey, queueNaming);
        }
        
        public void Clear()
            => _conventions.Clear();

        public void Dispose()
        {
            _conventions.Clear();
        }
    }
}