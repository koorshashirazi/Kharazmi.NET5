using Kharazmi.Dependency;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kharazmi.InMemory
{
    internal class InMemoryCache : MemoryCache, IMustBeInstance, IShouldBeSingleton
    {
        public InMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
        }

        public InMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) : base(
            optionsAccessor, loggerFactory)
        {
        }
    }
}