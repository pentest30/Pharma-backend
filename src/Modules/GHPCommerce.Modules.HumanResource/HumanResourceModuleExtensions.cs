using System;
using System.Reflection;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.MessageBrokers.RabbitMq;
using GHPCommerce.Modules.HumanResource.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Modules.HumanResource
{
    public static class HumanResourceModuleExtensions
    {
         public static IServiceCollection AddHumanResourceModuleDbContext(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
         {
             services.AddDbContext<HumanResourceDbContext>(options =>
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
                 .AddScoped(typeof(IRepository<Entities.Employee, Guid>),
                     typeof(Repository<Entities.Employee, Guid>));

             return services;

        }
        public static IServiceCollection AddHumanResourceModule(this IServiceCollection services, RabbitMqOptions options, string migrationsAssembly = "")
        {
            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
         
            return services;

        }
        public static void MigrateHumanResourceDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<HumanResourceDbContext>().Database.Migrate();
            }
        }
       
    }
}