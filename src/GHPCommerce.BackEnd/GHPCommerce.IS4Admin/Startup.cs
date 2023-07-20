using System.Threading;
using ElmahCore.Mvc;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.Cache;
using GHPCommerce.Infra.Mediator.Commands;
using GHPCommerce.IS4Admin.ConfigurationOptions;
using GHPCommerce.IS4Admin.Hubs;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace GHPCommerce.IS4Admin
{
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using AutoMapper;
    using Application;
    using GHPCommerce.Domain.Domain.Identity;
    using Infra.Identity;
    using Infra.OS;
    using Services;
    using Persistence;
    using IdentityServer4;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using System;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            AppSettings = new AppSettings();
            Configuration.Bind(AppSettings);
        }
       
        private AppSettings AppSettings { get; set; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ThreadPool.SetMinThreads(1000, 1000);

            IdentityModelEventSource.ShowPII = true;
            services.AddControllersWithViews();
            services.Configure<AppSettings>(Configuration);
            services.AddCors();
            services.AddCaches(AppSettings.Caching);
            services.AddDateTimeProvider();
            services.AddSingleton(AppSettings.MedIJKModel);
            services.AddSingleton(AppSettings.OpSettings);


            services.AddPersistence(AppSettings.ConnectionStrings.GHPCommerce)
                .AddIdentity();
            var path = Path.Combine(Directory.GetCurrentDirectory(), AppSettings.Certificates.Default.Path);
            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                })
                .AddSigningCredential(new X509Certificate2(path,
                    AppSettings.Certificates.Default.Password, X509KeyStorageFlags.EphemeralKeySet))
                .AddAspNetIdentity<User>()
                .AddIdServerPersistence(AppSettings.ConnectionStrings.GHPCommerce);
            services.AddPersistence(AppSettings.ConnectionStrings.GHPCommerce)
                .AddIdentityCore();
            // injects the current HTTP Context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUser, CurrentWebUser>();
            services.AddScoped<ISAuthorizedIdsUser, AuthorizedIdsUser>();
            services.AddApplication(AppSettings.MessageBroker);
            //services.AddApplication(default);
            services.AddCommandBus();
            services.AddScoped(typeof(ICurrentOrganization), typeof(OrganizationService));
           
            services.AddTransient<IProfileService, ProfileService>();
            var assemblies = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assemblies);
            var authenBuilder = services.AddAuthentication();
            if (AppSettings?.ExternalLogin?.Microsoft?.IsEnabled ?? false)
            {
                authenBuilder.AddMicrosoftAccount(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = AppSettings.ExternalLogin.Microsoft.ClientId;
                    options.ClientSecret = AppSettings.ExternalLogin.Microsoft.ClientSecret;
                });
            }

            if (AppSettings?.ExternalLogin?.Google?.IsEnabled ?? false)
            {
                authenBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = AppSettings.ExternalLogin.Google.ClientId;
                    options.ClientSecret = AppSettings.ExternalLogin.Google.ClientSecret;
                });
            }

            if (AppSettings?.ExternalLogin?.Facebook?.IsEnabled ?? false)
            {
                authenBuilder.AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.AppId = AppSettings.ExternalLogin.Facebook.AppId;
                    options.AppSecret = AppSettings.ExternalLogin.Facebook.AppSecret;
                });
            }

            //services.AddSingleton(typeof(IMqttService), typeof(MqttService));
            services.AddRazorPages();
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
         
            app.UseCors(
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
            // app.UseClassifiedAdsProfiler();
            // app.Map("/identity", authApp =>
            // {
            //     authApp.UseIdentityServer();
            //     app.UseStaticFiles("/identity");
            //     authApp.UsePathBase(new PathString("/identity"));
            //     authApp.UseIdentityServer();
            // });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHub<NotificationHub>("/hub");
            });
            //app.UseElmah();
            //var mqtService =app.ApplicationServices.GetService<IMqttService>();
            //try
            //{
            //    mqtService?.StartAsync();

            //} catch(Exception e)
            //{

            //}
        }
    }
}
