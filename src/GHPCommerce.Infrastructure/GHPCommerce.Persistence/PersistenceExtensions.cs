using EFCore.DbContextFactory.Extensions;
using ElmahCore.Mvc;
using ElmahCore.Sql;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace GHPCommerce.Persistence
{
    public static class PersistenceExtensions
    {
        // for testing purposes
        public static IServiceCollection AddInMemoryPersistence(this IServiceCollection services, string dbName)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName))
                .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
                .AddScoped(typeof(IUserRepository), typeof(UserRepository))
                .AddScoped(typeof(IRoleRepository), typeof(RoleRepository));
           
            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, sql =>
                {
                    if (!string.IsNullOrEmpty(migrationsAssembly))
                    {
                        sql.MigrationsAssembly(migrationsAssembly);
                    }

                }), ServiceLifetime.Transient)
                .AddTransient(typeof(IRepository<,>), typeof(Repository<,>))
                .AddScoped(typeof(IUserRepository), typeof(UserRepository))
                .AddScoped(typeof(IRoleRepository), typeof(RoleRepository));
            services.AddElmah<SqlErrorLog>(options =>
            {
                options.ConnectionString =connectionString; // DB structure see here: https://gist.githubusercontent.com/danielgreen/5671024/raw/6dc99d2d0f1397d33f36e99c840612bc4fb0b1e4/ELMAH-1.2-db-SQLServer.sql
                options.Path = @"elmah";
            });
            services.AddSqlServerDbContextFactory<ApplicationDbContext>();
            var sqlConnection = new ConnectionStrings(connectionString);
            services.AddSingleton(sqlConnection);
            return services;
        }

        public static void MigrateDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            }
        }
    }
}