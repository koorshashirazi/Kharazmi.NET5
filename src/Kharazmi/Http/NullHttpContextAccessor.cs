using Microsoft.AspNetCore.Http;

namespace Kharazmi.Http
{
    internal class NullHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; }
    }
}