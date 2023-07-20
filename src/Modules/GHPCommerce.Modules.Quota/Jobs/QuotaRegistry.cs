using System;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Modules.Quota.Jobs
{
    public class QuotaRegistry :Registry
    {
        public QuotaRegistry(IServiceProvider sp)
        {
            Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<ReleaseQuotaJob>())
                .ToRunEvery(1)
                .Days()
                .At(0, 0);
        }
    }
}