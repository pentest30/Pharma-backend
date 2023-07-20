using GHPCommerce.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Infra.OS
{
    public static class SemaphoreProviderExtensions
    {
        public static IServiceCollection AddSemaphoreProvider(this IServiceCollection services)
        {
            // _ = services.AddScoped(typeof(ILockProvider<>), typeof(ConcurrentDictionaryExtension<>));

            return services;
        }
    }
}
