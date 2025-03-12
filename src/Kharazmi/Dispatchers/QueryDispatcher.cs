#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Extensions;
using Kharazmi.Handlers;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Dispatchers
{
    internal class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceFactory;

        public QueryDispatcher(IServiceProvider serviceFactory)
           => _serviceFactory = serviceFactory;

        public async Task<TResult?> QueryAsync<TResult>(
            [NotNull] IQuery<TResult> query,
            MetadataCollection? metadata = null,
            CancellationToken token = default)
        {
            var handlerType = typeof(IQueryHandler<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            dynamic? handler = _serviceFactory.GetSafeService(handlerType);
            
            if (handler is null)
                return default;
            
            var domainContext = DomainMetadata.Empty;
            domainContext.AddRange(metadata);
            
            return await handler.HandleAsync((dynamic) query, domainContext, token);
        }
    }
}