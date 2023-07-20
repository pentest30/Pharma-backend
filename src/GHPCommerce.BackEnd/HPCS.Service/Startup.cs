using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Core.Shared.Services.ExternalServices;
using HPCS.Service.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GHPCommerce.Infra.Cache;
using HPCS.Service.OptionConfiguration;
using HPCS.Service.Services;
using Serilog;

namespace HPCS.Service
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AppSettings = new AppSettings();
            Configuration.Bind(AppSettings);
        }
        public IConfiguration Configuration { get; }

        public AppSettings AppSettings { get; set; }
        //private AppSettings AppSettings { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton(AppSettings);
            services.AddCaches(AppSettings.Caching);
            services.AddTransient<ICallApiService, CallApiService>();
            services.AddTransient<IHpcsService, HpcsService>();
            services.AddTransient<SalesOrderConsumer>();
            services.AddMassTransit(x =>
            {
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(AppSettings.MessageBroker.RabbitMq.Url, "/", sb =>
                    {
                        sb.Username(AppSettings.MessageBroker.RabbitMq.User);
                        sb.Password(AppSettings.MessageBroker.RabbitMq.Password);
                    });
                });
            });
            var logger = new LoggerConfiguration()
                .WriteTo
                .File("hpcs-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // services
            //     .AddHealthChecks()
            //     .AddRedis("localhost:6379");
            services.AddSingleton(logger);
            services.AddMassTransitHostedService();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
            GetBusControl(app);
            
            // app.UseHangfireDashboard();
        }

        private void GetBusControl(IApplicationBuilder app)
        {
            var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Durable = true;
               // cfg.ClearMessageDeserializers();
                //cfg.UseRawJsonSerializer();
                cfg.Host(AppSettings.MessageBroker.RabbitMq.Url, "/", sb =>
                {
                    sb.Username(AppSettings.MessageBroker.RabbitMq.User);
                    sb.Password(AppSettings.MessageBroker.RabbitMq.Password);
                });
                cfg.ReceiveEndpoint($"hpcs-order{AppSettings.ExternalApiInfo.OrganizationCode}", e => { e.Consumer<SalesOrderConsumer>(serviceScope?.ServiceProvider); });
            });
            busControl.StartAsync();
        }
    }
}
