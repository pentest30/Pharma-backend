using System;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Modules.Sales.Jobs
{
    public class SalesRegistry : Registry
    {
        // Inject the application service provider using pure DI.
        // (see Startup.cs code example)
        public SalesRegistry(IServiceProvider sp)
        {
            Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<DeletePendingOrdersJob>())
                .ToRunEvery(1)
                .Weeks()
                .On(DayOfWeek.Wednesday)
                .At(14, 45);
        }

    }
}