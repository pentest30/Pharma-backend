using System;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using EFCore.DbContextFactory.Extensions;
using FluentValidation;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.MessageBrokers.RabbitMq;
using GHPCommerce.Modules.Inventory.Commands;
using GHPCommerce.Modules.Inventory.Consumers;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Serilog;

namespace GHPCommerce.Modules.Inventory
{
    public static class InventoryModuleExtensions
    {
        // for testing purposes
        public static IServiceCollection AddInMemoryInventoryModuleDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<InventoryDbContext>(options => options.UseInMemoryDatabase(connectionString))
                .AddScoped(typeof(IRepository<InventSum, Guid>), typeof(Repository<InventSum, Guid>))
                .AddScoped(typeof(IInventoryRepository), typeof(InventoryRepository))
                .AddScoped(typeof(IRepository<InventItemTransaction, Guid>),
                    typeof(Repository<InventItemTransaction, Guid>))
                .AddScoped(typeof(IRepository<ZoneType, Guid>), typeof(Repository<ZoneType, Guid>))
               // .AddScoped(typeof(IZoneTypeRepository), typeof(ZoneTypeRepository))
                .AddScoped(typeof(ISequenceNumberService<TransferLog,Guid >), typeof(SequenceNumberService<TransferLog,Guid >)) ;

            return services;

        }
        public static IServiceCollection AddInventoryModuleDbContext(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(connectionString, sql =>
                {
                    if (!string.IsNullOrEmpty(migrationsAssembly))
                    {
                        sql.MigrationsAssembly(migrationsAssembly);
                    }

                }), ServiceLifetime.Transient)
                .AddTransient(typeof(IRepository<InventSum, Guid>), typeof(Repository<InventSum, Guid>))
                .AddTransient(typeof(IInventoryRepository), typeof(InventoryRepository))
                .AddTransient(typeof(IRepository<ZoneType, Guid>), typeof(Repository<ZoneType, Guid>))
                .AddTransient(typeof(IRepository<Batch, Guid>), typeof(Repository<Batch, Guid>))
                .AddTransient(typeof(IRepository<InventItemTransaction, Guid>), typeof(Repository<InventItemTransaction, Guid>))
                .AddTransient(typeof(IRepository<Invent, Guid>), typeof(Repository<Invent, Guid>))
                .AddTransient(typeof(IRepository<StockZone, Guid>), typeof(Repository<StockZone, Guid>))
                .AddTransient(typeof(IRepository<StockState, Guid>), typeof(Repository<StockState, Guid>))
                .AddTransient(typeof(IRepository<TransferLog, Guid>), typeof(Repository<TransferLog, Guid>))
                .AddTransient(typeof(ISequenceNumberService<TransferLog,Guid >), typeof(SequenceNumberService<TransferLog,Guid >));
              //  .AddScoped(typeof(IZoneTypeRepository), typeof(ZoneTypeRepository));
              services.AddSqlServerDbContextFactory<InventoryDbContext>();
              var sqlConnection = new ConnectionStrings(connectionString);
              services.AddSingleton(sqlConnection);
            return services;
            
        }

        public static IServiceCollection AddAxDbContext(this IServiceCollection services,string connectionString)
        {
            services.AddDbContext<AxDbContext>(options => options.UseSqlServer(connectionString)
                .ReplaceService<IQueryTranslationPostprocessorFactory, SqlServer2008QueryTranslationPostprocessorFactory>(), ServiceLifetime.Transient);
            services.AddSqlServerDbContextFactory<AxDbContext>();
            return services;
        }

        public static IServiceCollection AddInventoryModule(this IServiceCollection services)
        {
            var serilogger = new LoggerConfiguration()
                .WriteTo.File("ax_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
            services.AddScoped<InventoryConsumer>();
            services.AddSingleton(serilogger);
            return services;
        }

        public static void AddInventoryMasstransit(this IApplicationBuilder app, RabbitMqOptions options)
        {
            var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            
                var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(options.Url,  "/", h =>
                    {
                        h.Username(options.User);
                        h.Password(options.Password);
                    });
                    cfg.ReceiveEndpoint("invent-queue", e =>
                    {
                        e.Consumer<InventoryConsumer>(serviceScope?.ServiceProvider);
                    });
                });
                busControl.StartAsync();
        }
       

        public static void MigrateOnlineStoreDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.Migrate();
                //serviceScope.ServiceProvider.GetRequiredService<ICommandBus>().SendAsync(new SyncCachedInventSum());
            }
        }

        public static async Task SyncCachedInventSum(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            await serviceScope.ServiceProvider.GetRequiredService<ICommandBus>().SendAsync(new SyncCachedInventSum());
        }
    }

   
}