using System;
using GHPCommerce.Notification.Configuration;
using GHPCommerce.Notification.Saga;
using Hangfire;
using Hangfire.SqlServer;
using MassTransit;
using MassTransit.Saga;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Extensions;

namespace GHPCommerce.Notification
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
        private AppSettings AppSettings { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHostedMqttServer(mqttServer => mqttServer.WithoutDefaultEndpoint())
                .AddMqttConnectionHandler()
                .AddConnections();
         
            var machine = new DeliveryReceiptStateMachine();
            var deliveryOrderMachine = new DeliveryOrderStateMachine();
            var creditNoteMachine = new CreditNoteStateMachine();
            var repository = new InMemorySagaRepository<DeliveryReceiptState>();
            var repositoryDeliveryOrder = new InMemorySagaRepository<DeliveryOrderState>();
            var repositoryCreditNoteOrder = new InMemorySagaRepository<CreditNoteState>();

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {    
                cfg.Host( "localhost", "/", sb =>
                {
                    sb.Username("guest");
                    sb.Password("guest");
                });
                cfg.ReceiveEndpoint("order", e =>
                {
                    e.StateMachineSaga(machine, repository);
                    e.StateMachineSaga(deliveryOrderMachine, repositoryDeliveryOrder);
                    e.StateMachineSaga(creditNoteMachine, repositoryCreditNoteOrder);
                });
             
              
            });
            busControl.StartAsync();
            
            // services.AddHangfire(configuration => configuration
            //   //  .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            //     .UseSimpleAssemblyNameTypeSerializer()
            //     .UseRecommendedSerializerSettings()
            //     .UseSqlServerStorage(Configuration.GetConnectionString("SqlConnectionString"), new SqlServerStorageOptions
            //     {
            //         CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //         SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //         QueuePollInterval = TimeSpan.Zero,
            //         UseRecommendedIsolationLevel = true,
            //         DisableGlobalLocks = true
            //     }));
            //
            // // Add the processing server as IHostedService
            // services.AddHangfireServer();
            //services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                    endpoints.MapMqtt("/mqtt");
                });
            });
            app.UseMqttServer(server =>
            {
               
                // Todo: Do something with the server
            });
           // app.UseHangfireDashboard();
        }
    }
}
