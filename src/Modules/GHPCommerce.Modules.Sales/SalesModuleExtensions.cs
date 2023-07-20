using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Actors;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Jobs;
using GHPCommerce.Modules.Sales.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NLog.Web;
using Serilog;
using MassTransit;
using GHPCommerce.Infra.MessageBrokers.RabbitMq;
using GHPCommerce.Modules.Sales.Services;
using GHPCommerce.Persistence;

namespace GHPCommerce.Modules.Sales
{
    public static class SalesModuleExtensions
    {
        // for testing purposes
        public static IServiceCollection AddInMemorySalesModuleDbContext(this IServiceCollection services,
            string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<SalesDbContext>(options =>
                {
                    options.UseInMemoryDatabase(connectionString);
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }, ServiceLifetime.Transient)
                .AddScoped(typeof(IRepository<Order, Guid>), typeof(Repository<Order, Guid>))
                .AddScoped(typeof(IOrdersRepository), typeof(OrdersRepository))
                .AddScoped(typeof(IRepository<ShoppingCartItem, Guid>), typeof(Repository<ShoppingCartItem, Guid>))
                .AddScoped(typeof(IShoppingCartRepository), typeof(ShoppingCartRepository))
                .AddScoped(typeof(IDiscountsRepository), typeof(DiscountRepository))
                .AddScoped(typeof(IRepository<Discount, Guid>), typeof(Repository<Discount, Guid>))
                .AddScoped(typeof(IRepository<Invoice, Guid>), typeof(Repository<Invoice, Guid>))
                .AddScoped(typeof(IRepository<CreditNote, Guid>), typeof(Repository<CreditNote, Guid>))
                .AddScoped(typeof(IRepository<FinancialTransaction, Guid>), typeof(Repository<FinancialTransaction, Guid>));
                
            return services;
        }

        public static IServiceCollection AddSalesModuleDbContext(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<SalesDbContext>(options =>
                {
                    options.UseSqlServer(connectionString, sql =>
                    {
                        if (!string.IsNullOrEmpty(migrationsAssembly))
                        {
                            sql.MigrationsAssembly(migrationsAssembly);
                        }
                    });
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }, ServiceLifetime.Transient)
                .AddTransient(typeof(IRepository<Order, Guid>), typeof(Repository<Order, Guid>))
                .AddTransient(typeof(IOrdersRepository), typeof(OrdersRepository))
                .AddTransient(typeof(IRepository<ShoppingCartItem, Guid>), typeof(Repository<ShoppingCartItem, Guid>))
                .AddTransient(typeof(IShoppingCartRepository), typeof(ShoppingCartRepository))
                .AddTransient(typeof(IRepository<Discount, Guid>), typeof(Repository<Discount, Guid>))
                .AddTransient(typeof(IDiscountsRepository), typeof(DiscountRepository))
                .AddTransient(typeof(IRepository<Invoice, Guid>), typeof(Repository<Invoice, Guid>))
                .AddTransient(typeof(IRepository<CreditNote, Guid>), typeof(Repository<CreditNote, Guid>))
                .AddTransient(typeof(IRepository<FinancialTransaction, Guid>), typeof(Repository<FinancialTransaction, Guid>))
                .AddTransient(typeof(ISequenceNumberService<Invoice, Guid>),
                typeof(SequenceNumberService<Invoice, Guid>))
                .AddTransient(typeof(ISequenceNumberService<CreditNote, Guid>),
                typeof(SequenceNumberService<CreditNote, Guid>));
            var sqlConnection = new ConnectionStrings(connectionString);
            services.AddSingleton(sqlConnection);
            return services;

        }
        public static IServiceCollection AddSalesModule(this IServiceCollection services, string migrationsAssembly = "")
        {
            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
            services.AddSignalR();
            services.AddSingleton<ISerialQueue, SerialQueue >();
            services.AddSingleton<IFileProvider>(   new PhysicalFileProvider(Directory.GetCurrentDirectory()));  
            //  services.AddSingleton<ILoggerManager, LoggerManager>();
            var logger = new LoggerConfiguration()
                .WriteTo.File("ax_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            services.AddSingleton(logger);
            services.AddTransient<DeletePendingOrdersJob>();
            services.AddSingleton<IServiceReleaseQuantities, ServiceReleaseQuantities>();
          
            return services;

        }
        public static void MigrateSalesDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<SalesDbContext>().Database.Migrate();
            }
        }
        // add receiver bus 
        public static void AddSalesMasstransit(this IApplicationBuilder app, RabbitMqOptions options)
        {

            var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(options.Url, "/", h =>
                {
                    h.Username(options.User);
                    h.Password(options.Password);

                }); 
            });
            busControl.StartAsync();
        }


    }
}
