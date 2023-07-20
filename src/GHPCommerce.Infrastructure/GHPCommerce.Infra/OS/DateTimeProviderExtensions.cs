using GHPCommerce.CrossCuttingConcerns.OS;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Infra.OS
{
    public static class DateTimeProviderExtensions
    {
        public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
        {
            _ = services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            return services;
        }
    }
}
