using System;
using Kharazmi.Dependency;
using Microsoft.Extensions.Localization;

namespace Kharazmi.Mvc.Globalization
{
    public class NullStringLocalizerFactory : IStringLocalizerFactory, INullInstance
    {
        public IStringLocalizer Create(Type resourceSource)
            => new NullStringLocalizer();

        public IStringLocalizer Create(string baseName, string location)
            => new NullStringLocalizer();
    }
}