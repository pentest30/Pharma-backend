using System.IO;
using AutoMapper;
using GHPCommerce.Application;
using GHPCommerce.Infra.Identity;
using GHPCommerce.Infra.OS;
using GHPCommerce.Migrator;
using GHPCommerce.Modules.Inventory;
using GHPCommerce.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GHPCommerce.Modules.Sales.Test
{
    public class ServiceFixture
    {
        public ServiceProvider ServiceProvider { get; }

        public ServiceFixture()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", true, true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var startup = new Startup(configuration);
            serviceCollection.AddAutoMapper(typeof(ApplicationServicesExtensions),
                typeof(InventoryModuleExtensions),
                typeof(SalesModuleExtensions));
            serviceCollection.AddDateTimeProvider();
            var moqHttContext = new Mock<IHttpContextAccessor>();
            serviceCollection.AddSingleton(moqHttContext.Object);
            serviceCollection.AddInMemoryInventoryModuleDbContext("GHPCommerceDb");
            serviceCollection.AddInMemorySalesModuleDbContext("GHPCommerceDb");
            serviceCollection.AddInMemoryPersistence("GHPCommerceDb");
            serviceCollection.AddLogging();
            serviceCollection.AddIdentity();
            serviceCollection.AddApplication(default);
            serviceCollection.AddInventoryModule();
            serviceCollection.AddSalesModule();
            //serviceCollection.AddScoped(typeof(ICurrentOrganization), typeof(OrganizationService));
            serviceCollection.AddSemaphoreProvider();
            ServiceProvider = serviceCollection.BuildServiceProvider();
            startup.ConfigureServices(serviceCollection);
        }

    }
}
