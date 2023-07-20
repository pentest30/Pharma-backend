using System;
using System.IO;
using System.Reflection;
using FluentValidation;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Infra.Cache;
using GHPCommerce.Infra.MessageBrokers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GHPCommerce.Application
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, MessageBrokerOptions config, CachingOptions options = null)
        {
            var serilogger = new LoggerConfiguration()
                .WriteTo.File("ax_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            var assemblies = Assembly.GetExecutingAssembly();
            //services.AddAutoMapper(assemblies);
            services.AddValidatorsFromAssembly(assemblies);
            services.AddMediatR(assemblies);
            services.AddSignalR();
            services.AddSingleton(serilogger);
            return services;
        }

    }
}