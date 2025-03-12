#region

using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

#endregion

namespace Kharazmi.Hangfire
{
    internal class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext filterContext, IWriteOnlyTransaction transaction)
        {
            filterContext.JobExpirationTimeout = TimeSpan.FromHours(7); //TODO
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromHours(7); //TODO
        }
    }
}