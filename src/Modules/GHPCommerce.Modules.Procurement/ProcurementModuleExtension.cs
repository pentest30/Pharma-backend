using System;
using System.Reflection;
using AutoMapper;
using EFCore.DbContextFactory.Extensions;
using FluentValidation;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.MessageBrokers.RabbitMq;
using GHPCommerce.Modules.Procurement.Consumers;
using GHPCommerce.Modules.Procurement.Entities;
using GHPCommerce.Modules.Procurement.Repositories;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace GHPCommerce.Modules.Procurement
{
    public static class ProcurementModuleExtension
    {
        public static IServiceCollection AddProcurementModuleDbContext(this IServiceCollection services,string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<ProcurementDbContext>(options =>
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
                })  
                .AddScoped(typeof(IRepository<SupplierOrder,Guid >), typeof(Repository<SupplierOrder,Guid>))
                .AddScoped(typeof(ISupplierOrderRepository), typeof(SupplierOrderRepository)) 
                .AddScoped(typeof(IRepository<SupplierInvoice,Guid >), typeof(Repository<SupplierInvoice,Guid>))
                .AddScoped(typeof(ISupplierInvoiceRepository), typeof(SupplierInvoiceRepository))
                .AddScoped(typeof(IRepository<DeliveryReceipt,Guid >), typeof(Repository<DeliveryReceipt,Guid>))
                .AddScoped(typeof(IDeliveryReceiptRepository), typeof(DeliveryReceiptRepository))
                .AddScoped(typeof(ISequenceNumberService<DeliveryReceipt,Guid >), typeof(SequenceNumberService<DeliveryReceipt,Guid >))
                .AddScoped(typeof(ISequenceNumberService<SupplierOrder,Guid >), typeof(SequenceNumberService<SupplierOrder,Guid >))
                .AddScoped(typeof(ISequenceNumberService<SupplierInvoice,Guid >), typeof(SequenceNumberService<SupplierInvoice,Guid >));

            services.AddSqlServerDbContextFactory<ProcurementDbContext>();
            return services;
        }

        public static IServiceCollection AddProcurementModule(this IServiceCollection services, RabbitMqOptions options, string migrationsAssembly = "")
        {
            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
            // services.AddSignalR();
            // AddMasstransitConfig(services, options);
            // var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            // services.AddSingleton(logger);
            services.AddScoped<DeliveryReceiptConsumer>();
            return services;
        }

        public static void MigrateProcurementDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<ProcurementDbContext>().Database.Migrate();
            }
        }
        // add receiver bus 
        public static void AddProcurementMasstransit(this IApplicationBuilder app, RabbitMqOptions options)
        {
           
            var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(options.Url,  "/", h =>
                {
                    h.Username(options.User);
                    h.Password(options.Password);
                        
                });
                cfg.ReceiveEndpoint("procurement-queue", e =>
                {
                    e.Consumer<DeliveryReceiptConsumer>(serviceScope?.ServiceProvider);
                });
            });
            busControl.StartAsync();
         }
        // add publisher bus
        static void AddMasstransitConfig(IServiceCollection services, RabbitMqOptions options)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(options.Url,  "/", h =>
                    {
                        h.Username(options.User);
                        h.Password(options.Password);
                    });
                });
            });
            services.AddMassTransitHostedService();
        }

    }
}