using System.Reflection;
using AutoMapper;
using FluentValidation.AspNetCore;
using GHPCommerce.Application;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Core.Shared.Services.ExternalServices;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.Cache;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Identity;
using GHPCommerce.Infra.Mediator.Commands;
using GHPCommerce.Infra.Notification.Email;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.HumanResource;
using GHPCommerce.Modules.Inventory;
using GHPCommerce.Modules.Quota;
using GHPCommerce.Modules.Quota.Hubs;
using GHPCommerce.Modules.Sales;
using GHPCommerce.Modules.PreparationOrder;
using GHPCommerce.Modules.Procurement;
using GHPCommerce.Modules.Procurement.Hubs;
using GHPCommerce.Modules.Sales.Hubs;
using GHPCommerce.Persistence;
using GHPCommerce.WebApi.ConfigurationOptions;
using GHPCommerce.WebApi.Hepers;
using GHPCommerce.WebApi.Middleware;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace GHPCommerce.WebApi
{
    public class  Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AppSettings = new AppSettings();
            Configuration.Bind(AppSettings);
        }
        public IConfiguration Configuration { get; }
        private AppSettings AppSettings { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ThreadPool.SetMinThreads(1000, 1000);
            // System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            IdentityModelEventSource.ShowPII = true;
            services.AddControllers();
            services.AddSingleton(AppSettings);
            // add modules
            services.AddCaches(AppSettings.Caching);
            // add swagger service
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GHP Commerce api v1", Version = "v1" });
            });
            // add automapper config
            services.AddAutoMapper(typeof(Startup),
                typeof(InventoryModuleExtensions),
                typeof(ApplicationServicesExtensions),
                typeof(SalesModuleExtensions),
                typeof(QuotaModuleExtensions),
                typeof(PreparationOrderModuleExtensions),
                typeof(ProcurementModuleExtension),
                typeof(HumanResourceModuleExtensions));

            // add CORS config
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedOrigins", builder => builder
                    .WithOrigins(AppSettings.CORS.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader());

                options.AddPolicy("AllowAnyOrigin", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

                options.AddPolicy("CustomPolicy", builder => builder
                    .AllowAnyOrigin()
                    .WithMethods("Get")
                    .WithHeaders("Content-Type"));
            });
            services.AddDateTimeProvider();
            // add persistence dependencies
            services.AddPersistence(AppSettings.ConnectionStrings.SqlConnectionString)
                .AddIdentityCore();
            services.AddInventoryModuleDbContext(AppSettings.ConnectionStrings.SqlConnectionString);
            services.AddSingleton(AppSettings.PreparationInventEndPoint);
            // un db context pour la consultation des arrivages 
            if (AppSettings.MedIJKModel.AXInterfacing)
            {
                services.AddAxDbContext(AppSettings.ConnectionStrings.AxConnectionString);
            }
            services.AddSalesModuleDbContext(AppSettings.ConnectionStrings.SqlConnectionString);
            services.AddQuotaModuleDbContext(AppSettings.ConnectionStrings.SqlConnectionString);
            services.AddPreparationOrderModuleDbContext(AppSettings.ConnectionStrings.SqlConnectionString);
            services.AddProcurementModuleDbContext(AppSettings.ConnectionStrings.SqlConnectionString);
            services.AddHumanResourceModuleDbContext(AppSettings.ConnectionStrings.SqlConnectionString);

          
            // add the config of Identity server 4 middle ware 
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = AppSettings.IdentityServerAuthentication.Authority;
                    options.ApiName = AppSettings.IdentityServerAuthentication.ApiName;
                    options.RequireHttpsMetadata = AppSettings.IdentityServerAuthentication.RequireHttpsMetadata;
                    options.TokenRetriever = CustomTokenRetriever.FromHeaderAndQueryString;

                });
            // injects the current HTTP Context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUser, CurrentWebUser>();
            services.AddScoped(typeof(ICurrentOrganization), typeof(OrganizationService));
            services.AddScoped(typeof(ICacheService), typeof(CacheService));
            services.AddTransient<ICallApiService, CallApiService>();
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            services.AddSingleton(logger);
            services.AddApplication(AppSettings.MessageBroker, AppSettings.Caching);
            services.AddInventoryModule();
            services.AddSalesModule();
            services.AddQuotaModule();
            services.AddPreparationOrderModule(AppSettings.MessageBroker.RabbitMq);
            services.AddProcurementModule(AppSettings.MessageBroker.RabbitMq);
            services.AddHumanResourceModule(AppSettings.MessageBroker.RabbitMq);
            // add sendGrid api
            services.AddSendGridEmailApi(AppSettings.SenderOptions);
            services.AddSingleton(AppSettings.PrinterOptions);
            services.AddSingleton(AppSettings.MedIJKModel);
            services.AddSingleton(AppSettings.OpSettings);
            services.AddSingleton(AppSettings.DeptServiceConfig);
            services.AddSingleton(AppSettings.MessageBroker.RabbitMq);
            services.AddSemaphoreProvider();
            //services.AddSignalR();
            // add model validator filter
            services.AddMvc(options =>
                {
                    //options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidateModelStateFilter));

                })
                .ConfigureApiBehaviorOptions(op =>
                {
                    // disable the default response from web api .
                    op.SuppressModelStateInvalidFilter = true;
                })
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
                .AddJsonOptions(
                    options => options.JsonSerializerOptions.IgnoreNullValues = false
                );
            var serilogger = new LoggerConfiguration()
                .WriteTo.File("httprequest_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // services
            //     .AddHealthChecks()
            //     .AddRedis("localhost:6379");
            services.AddSingleton(serilogger);
            services.AddCommandBus();
            // services.AddMetrics();

            //EntityFrameworkProfiler.Initialize();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(AppSettings.CORS.AllowAnyOrigin ? "AllowAnyOrigin" : "AllowedOrigins");
            app.UseSwagger();
            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "GHPCommerce API");
                setupAction.RoutePrefix = string.Empty;
                setupAction.DefaultModelExpandDepth(2);
                setupAction.DefaultModelRendering(ModelRendering.Model);
                setupAction.DocExpansion(DocExpansion.None);
                setupAction.EnableDeepLinking();
                setupAction.DisplayOperationId();
            });

            app.UseAuthentication();
            app.UseAuthorization();
            // app.UseElmah();
            //app.UseSession();
            app.UseMiddleware<RequestHandlerMiddleware>();
            // app.UseMiddleware<ExceptionMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    // ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHub<InventSumHub>("/invent-hub", options =>
                {
                    options.Transports =
                        HttpTransportType.WebSockets |
                        HttpTransportType.LongPolling;
                });
                endpoints.MapHub<QuotaNotification>("/quota-hub", options =>
                {
                    options.Transports =
                        HttpTransportType.WebSockets |
                        HttpTransportType.LongPolling;
                });
                ;
                endpoints.MapHub<ProcurementHub>("/procurement-hub", options =>
                {
                    options.Transports =
                        HttpTransportType.WebSockets |
                        HttpTransportType.LongPolling;
                });
            });

            await app.SyncCachedInventSum().ConfigureAwait(false);
            app.AddProcurementMasstransit(AppSettings.MessageBroker.RabbitMq);
            app.AddInventoryMasstransit(AppSettings.MessageBroker.RabbitMq);
            app.AddPreparationOrderMasstransit(AppSettings.MessageBroker.RabbitMq);
            app.AddSalesMasstransit(AppSettings.MessageBroker.RabbitMq);
            //JobManager.Initialize(new SalesRegistry(app.ApplicationServices));
            // app.UseMiddleware<RequestHandlerMiddleware>();
            //JobManager.Initialize(new QuotaRegistry(app.ApplicationServices));
            app.Use(async (context, next) =>
            {
                //  Here to get token from url parameter, the actual practical application please consider adding some filter conditions
                if (context.Request.Query.TryGetValue("token", out var token))
                {
                    //  Before getting the url from the header, the header being added to, must be in UseAuthentication
                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
                }

                await next.Invoke();
            });

        }
    }
}
