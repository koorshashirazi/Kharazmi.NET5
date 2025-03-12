using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Kharazmi.OpenIdConnect.Default
{
    internal class NullClaimsTransformation:IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
            => Task.FromResult(new ClaimsPrincipal());
    }
}