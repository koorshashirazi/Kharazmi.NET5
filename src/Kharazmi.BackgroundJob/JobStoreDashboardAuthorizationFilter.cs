#region

using System;
using Hangfire.Dashboard;
using Kharazmi.BuilderExtensions;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Options.Hangfire;
using Kharazmi.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Hangfire
{
    internal class JobStoreDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var services = context.GetHttpContext().RequestServices;
            var authorization = services.GetRequiredService<IAuthorizationService>();
            var jobStoreOptions = services.GetSettings().Get<HangfireOption>();
            var logger = services.GetService<ILogger<JobStoreDashboardAuthorizationFilter>>();

            if (jobStoreOptions.Enable == false || jobStoreOptions.PolicyName.IsEmpty())
            {
                logger?.LogError("From Hangfire: Invalid jobStoreOptions, Can not find BackgroundJobPolicy");
                if (logger is null)
                    throw new FrameworkException("From Hangfire: Invalid jobStoreOptions, Can not find BackgroundJobPolicy");
                return false;
            }
            var user = context.GetHttpContext()?.User;
            if (user is null)
            {
                logger?.LogError("From Hangfire: Invalid claims principal");
                return false;
            }
            try
            {
                var allowed = AsyncHelper.RunSync(async () =>
                    await authorization.AuthorizeAsync(user, jobStoreOptions.PolicyName));
                logger?.LogTrace("From Hangfire: JobStore authorization allowed: {Succeeded}", allowed.Succeeded);
                return allowed.Succeeded;
            }
            catch (Exception e)
            {
                logger?.LogError("From Hangfire: Can not find BackgroundJobPolicy");
                if (logger is null)
                    throw new FrameworkException(e.Message, e);
                return false;
            }
          
        }
    }
}