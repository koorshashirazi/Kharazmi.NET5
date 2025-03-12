using System;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Mvc.Localization;

namespace Kharazmi.Mvc.Globalization
{
    internal class NullHtmlLocalizerFactory : IHtmlLocalizerFactory, INullInstance

    {
        public IHtmlLocalizer Create(Type resourceSource)
            => new NullHtmlLocalizer();

        public IHtmlLocalizer Create(string baseName, string location)
            => new NullHtmlLocalizer();
    }
}