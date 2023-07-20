using System.Collections.Generic;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GHPCommerce.Persistence
{
    public static class IdServerPersistenceExtensions
    {
        public static IIdentityServerBuilder AddIdServerPersistence(this IIdentityServerBuilder services, string connectionString, string migrationsAssembly = "")
        {
            services.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql =>
                        {
                            if (!string.IsNullOrEmpty(migrationsAssembly))
                            {
                                sql.MigrationsAssembly(migrationsAssembly);
                            }
                        });
                options.DefaultSchema = "ids";
            })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql =>
                            {
                                if (!string.IsNullOrEmpty(migrationsAssembly))
                                {
                                    sql.MigrationsAssembly(migrationsAssembly);
                                }
                            });
                    options.DefaultSchema = "ids";
                    
                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30; // interval in seconds
                });
            return services;
        }

        public static void MigrateIdServerDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                var clients = new List<Client>();
                if (!context.Clients.Any(x => x.ClientId == "GHPCommerce.spa"))
                {
                    clients.Add(new Client
                    {
                        ClientId = "GHPCommerce.spa",
                        ClientName = "GHPCommerce spa",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        RedirectUris =
                        {
                            "http://localhost:4200/oidc-login-redirect",
                            "http://localhost:4200/auth-callback",
                        },
                        PostLogoutRedirectUris =
                        {
                            "http://localhost:4200/",
                        },
                        AllowedCorsOrigins =
                        {
                            "http://localhost:4200",
                        },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "GHPCommerce.WebApi"
                        },
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256()),
                        },
                        AllowOfflineAccess = true
                    });
                }
                if (!context.Clients.Any(x => x.ClientId == "GHPCommerce.Ecommerce"))
                {
                    clients.Add(new Client
                    {
                        ClientId = "GHPCommerce.Ecommerce",
                        ClientName = "GHPCommerce Web MVC",
                        AllowedGrantTypes = GrantTypes.Hybrid.Combines(GrantTypes.ResourceOwnerPassword),
                        RedirectUris =
                        {
                            "https://localhost:44364/signin-oidc",
                            "http://host.docker.internal:9003/signin-oidc",
                        },
                        PostLogoutRedirectUris =
                        {
                            "https://localhost:44364/signout-callback-oidc",
                            "http://host.docker.internal:9003/signout-callback-oidc",
                        },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "GHPCommerce.WebAPI",
                        },
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256()),
                        },
                        AllowOfflineAccess = true
                    });
                }

                if (!context.Clients.Any(x => x.ClientId == "GHPCommerce.Flutter"))
                {
                    clients.Add(new Client
                    {
                        ClientId = "GHPCommerce.Flutter",
                        ClientName = "GHPCommerce Flutter",
                        AllowedGrantTypes = GrantTypes.Hybrid.Combines(GrantTypes.ResourceOwnerPassword),
                        RedirectUris =
                        {
                            "https://localhost:4000/",
                            "https://localhost:4000/",
                        },
                        PostLogoutRedirectUris =
                        {
                            "https://localhost:4000/signout-callback-oidc",
                            "http://host.docker.internal:9003/signout-callback-oidc",
                        },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "GHPCommerce.WebAPI",
                        },
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256()),
                        },
                        AllowOfflineAccess = true
                    });
                }
                if (!context.Clients.Any(x => x.ClientId == "GHPCommerce.Mobile"))
                {
                    clients.Add( new Client
                        {
                            ClientId = "GHPCommerce.Mobile",
                            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                            ClientSecrets =
                            {
                                new Secret("secret".Sha256())
                            },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "GHPCommerce.WebApi",
                        },
                    }
                    );
                }

                if (clients.Any())
                {
                    context.Clients.AddRange(clients.Select(x => x.ToEntity()));
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    var identityResources = new List<IdentityResource>()
                    {
                        new IdentityResources.OpenId(),
                       // new ProfileWithRoleIdentityResource(),
                        new IdentityResources.Profile(),
                    };

                    context.IdentityResources.AddRange(identityResources.Select(x => x.ToEntity()));

                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var apiResources = new List<ApiResource>
                    {
                        new ApiResource("GHPCommerce.WebApi", "GHPCommerce Web API",
                        new List<string> { "role" }),
                    };

                    context.ApiResources.AddRange(apiResources.Select(x => x.ToEntity()));
                    context.SaveChanges();
                }
            }
        }
    }
}
