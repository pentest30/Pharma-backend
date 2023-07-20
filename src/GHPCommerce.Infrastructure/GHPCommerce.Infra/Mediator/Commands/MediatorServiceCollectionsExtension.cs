using GHPCommerce.Domain.Domain.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Infra.Mediator.Commands
{
    public static class MediatorServiceCollectionsExtension
    {
        public static IServiceCollection AddCommandBus(this IServiceCollection services)
        {
            services.AddTransient<ICommandBus, CommandBus>();
            return services;
        }
    }
}
