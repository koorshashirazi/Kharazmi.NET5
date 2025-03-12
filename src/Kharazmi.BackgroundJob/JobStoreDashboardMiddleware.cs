#region

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Kharazmi.Configuration;
using Kharazmi.Options.Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

#endregion

namespace Kharazmi.Hangfire
{
    /// <summary> </summary>
    public class JobStoreDashboardMiddleware
    {
        #region Ctor

        /// <summary> </summary>
        public JobStoreDashboardMiddleware(
            RequestDelegate nextRequestDelegate,
            JobStorage storage,
            DashboardOptions options,
            RouteCollection routes,
            ISettingProvider settingProvider)
        {
            _nextRequestDelegate = nextRequestDelegate;
            _jobStorage = storage;
            _dashboardOptions = options;
            _routeCollection = routes;
            _settingProvider = settingProvider;
        }

        #endregion

        /// <summary> </summary>
        public async Task Invoke(HttpContext httpContext)
        {
            var option = _settingProvider.Get<HangfireOption>();
            if(option.Enable == false)
            {
                await _nextRequestDelegate.Invoke(httpContext);
                return;
            }

            if (option.UseDashboard == false)
            {
                await _nextRequestDelegate.Invoke(httpContext);
                return;
            }  
            
            var dashboardContext =
                new AspNetCoreDashboardContext(_jobStorage, _dashboardOptions, httpContext);

            var findResult = _routeCollection.FindDispatcher(httpContext.Request.Path.Value);

            if (findResult is null)
            {
                await _nextRequestDelegate.Invoke(httpContext);
                return;
            }

            if (option.UseAuthorization)
            {
                if (httpContext.User.Identity is null || !httpContext.User.Identity.IsAuthenticated)
                {
                    var prop = new AuthenticationProperties
                    {
                        RedirectUri = option.PathMatch
                    };
                    await httpContext.ChallengeAsync(prop);
                    return;
                }

                if (_dashboardOptions.Authorization.Any(filter => filter.Authorize(dashboardContext) == false))
                {
                    var isAuthenticated = httpContext.User.Identity?.IsAuthenticated;
                    httpContext.Response.StatusCode = isAuthenticated == true
                        ? (int) HttpStatusCode.Forbidden
                        : (int) HttpStatusCode.Unauthorized;
                    return;
                }
            }


            dashboardContext.UriMatch = findResult.Item2;
            await findResult.Item1.Dispatch(dashboardContext);
        }

        #region Private

        private readonly DashboardOptions _dashboardOptions;
        private readonly JobStorage _jobStorage;
        private readonly RequestDelegate _nextRequestDelegate;
        private readonly RouteCollection _routeCollection;
        private readonly ISettingProvider _settingProvider;

        #endregion
    }
}