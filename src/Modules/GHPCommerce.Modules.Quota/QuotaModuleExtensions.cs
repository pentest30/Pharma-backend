using System;
using System.Reflection;
using AutoMapper;
using EFCore.DbContextFactory.Extensions;
using FluentValidation;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Jobs;
using GHPCommerce.Modules.Quota.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;

namespace GHPCommerce.Modules.Quota
{
    public static class QuotaModuleExtensions
    {
        public static IServiceCollection AddQuotaModuleDbContext(this IServiceCollection services,string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<QuotaDbContext>(options =>
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
                .AddTransient(typeof(IRepository<QuotaInitState, Guid>), typeof(Repository<QuotaInitState, Guid>))
                .AddTransient(typeof(IRepository<Entities.Quota, Guid>), typeof(Repository<Entities.Quota, Guid>))
                .AddTransient(typeof(IQuotaRepository), typeof(QuotaRepository))
                .AddTransient(typeof(IRepository<QuotaRequest, Guid>), typeof(Repository<QuotaRequest, Guid>))
                .AddTransient(typeof(IQuotaRequestRepository), typeof(QuotaRequestRepository));
            services.AddSqlServerDbContextFactory<QuotaDbContext>();
           // services.AddTransient<ReleaseQuotaJob>();
            //services.AddTransient<ReleaseQuotaJob>();
            return services;

        }

        public static IServiceCollection AddQuotaModule(this IServiceCollection services)
        {
            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
            services.AddSignalR();
            
            return services;
        }

        public static void MigrateQuotaDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<QuotaDbContext>().Database.Migrate();
            }
        }
        
    }
}
