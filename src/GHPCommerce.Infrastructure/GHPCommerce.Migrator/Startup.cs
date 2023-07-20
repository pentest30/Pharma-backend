using System.Reflection;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Infra.Identity;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.HumanResource;
using GHPCommerce.Modules.Inventory;
using GHPCommerce.Modules.PreparationOrder;
using GHPCommerce.Modules.Procurement;
using GHPCommerce.Modules.Quota;
using GHPCommerce.Modules.Sales;
using GHPCommerce.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GHPCommerce.Migrator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
          
            services.AddDateTimeProvider();
            services.AddScoped<ICurrentUser, CurrentWebUser>();
            services.AddInventoryModuleDbContext(Configuration["ConnectionStrings:GHPCommerce"],
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddSalesModuleDbContext(Configuration["ConnectionStrings:GHPCommerce"],
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddQuotaModuleDbContext(Configuration["ConnectionStrings:GHPCommerce"],
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddPreparationOrderModuleDbContext(Configuration["ConnectionStrings:GHPCommerce"],
               typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddPersistence(Configuration["ConnectionStrings:GHPCommerce"],
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddProcurementModuleDbContext(Configuration["ConnectionStrings:GHPCommerce"],
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddHumanResourceModuleDbContext(Configuration["ConnectionStrings:GHPCommerce"],
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            services.AddIdentityServer()
                .AddIdServerPersistence(Configuration.GetConnectionString("GHPCommerce"),
                    typeof(Startup).GetTypeInfo().Assembly.GetName().Name);


        }

        // This method gets called by the runtime. Use this method to configure  the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.MigrateDb();
            app.MigrateIdServerDb();
            app.MigrateOnlineStoreDb();
            app.MigrateSalesDb();
            app.MigrateQuotaDb();
            app.MigratePreparationOrderDb();
            app.MigrateProcurementDb();
            app.MigrateHumanResourceDb();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
