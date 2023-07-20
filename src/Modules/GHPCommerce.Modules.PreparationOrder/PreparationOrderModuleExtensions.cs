using System;
using System.Reflection;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.MessageBrokers.RabbitMq;
using GHPCommerce.Modules.PreparationOrder.Consumers;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using GHPCommerce.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Serilog;

namespace GHPCommerce.Modules.PreparationOrder
{
    public static class PreparationOrderModuleExtensions
    {
        public static IServiceCollection AddPreparationOrderModuleDbContext(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<PreparationOrderDbContext>(options =>
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
                .AddTransient(typeof(IRepository<Entities.PreparationOrder, Guid>),
                    typeof(Repository<Entities.PreparationOrder, Guid>))
                .AddTransient(typeof(IPreparationOrderRepository), typeof(PreparationOrderRepository))
                .AddTransient(typeof(IRepository<ConsolidationOrder, Guid>), typeof(Repository<ConsolidationOrder, Guid>))
                .AddTransient(typeof(IRepository<DeleiveryOrder, Guid>), typeof(Repository<DeleiveryOrder, Guid>))
                .AddTransient(typeof(ISequenceNumberService<DeleiveryOrder, Guid>),
                    typeof(SequenceNumberService<DeleiveryOrder, Guid>))
                .AddTransient(typeof(ISequenceNumberService<Entities.PreparationOrder, Guid>),
                    typeof(SequenceNumberService<Entities.PreparationOrder, Guid>));

            var sqlConnection = new ConnectionStrings(connectionString);
            services.AddSingleton(sqlConnection);
            return services;

        }
        public static IServiceCollection AddPreparationOrderModule(this IServiceCollection services, RabbitMqOptions options, string migrationsAssembly = "")
        {
            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
            var serilogger = new LoggerConfiguration()
                .WriteTo.File("ax_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            services.AddSingleton(serilogger);
            //  services.AddSignalR();
            // services.AddSingleton<ISerialQueue, SerialQueue>();
            //  services.AddSingleton<ILoggerManager, LoggerManager>();
            // var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            // services.AddSingleton(logger);
            AddMasstransitConfig(services, options);
            services.AddScoped<DeliveryOrderConsumer>();
            return services;

        }
        public static void MigratePreparationOrderDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PreparationOrderDbContext>().Database.Migrate();
            }
        }
        // add receiver bus 
        public static void AddPreparationOrderMasstransit(this IApplicationBuilder app, RabbitMqOptions options)
        {
           
            var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(options.Url,  "/", h =>
                {
                    h.Username(options.User);
                    h.Password(options.Password);
                        
                });
                cfg.ReceiveEndpoint("preparation-order-queue", e =>
                {
                    e.Consumer<DeliveryOrderConsumer>(serviceScope?.ServiceProvider);
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
