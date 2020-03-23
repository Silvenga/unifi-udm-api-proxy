using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UdmApi.Proxy.Data;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Services;
using UdmApi.Proxy.Sessions;

namespace UdmApi.Proxy
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            services.AddTransient<ProtectLoginProxy>();
            services.AddTransient<ProtectAccessKeyProxy>();
            services.AddTransient<ProtectProxy>();
            services.AddTransient<ProtectCameraProxy>();

            services.AddSingleton<IProxyHttpClientFactory, ProxyHttpClientFactory>();
            services.AddSingleton<ISsoSessionCache, DatabaseSsoSessionCache>();

            services.AddDbContext<ApplicationContext>(
                options =>
                {
                    var databasePath = GetDatabasePath();
                    options.UseSqlite($"Data Source={databasePath}");
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/healthcheck");

            app.AddServiceProxy<ProtectLoginProxy>();
            app.AddServiceProxy<ProtectAccessKeyProxy>();
            app.AddServiceProxy<ProtectProxy>();
            app.AddServiceProxy<ProtectCameraProxy>();

            app.UseMiddleware<FallbackMiddleware>();

            logger.LogInformation($"Checking if database '{GetDatabasePath()}' requires migrations.");
            MigrateDatabase<ApplicationContext>(app, logger);
            logger.LogInformation("Migration completed.");
        }

        private string GetDatabasePath()
        {
            var databasePath = _configuration?["Database"] ?? "data.db";
            var fullyQualifiedPath = Path.GetFullPath(databasePath);
            return fullyQualifiedPath;
        }

        private static void MigrateDatabase<T>(IApplicationBuilder app, ILogger<Startup> logger) where T : DbContext
        {
            using var serviceScope = app.ApplicationServices
                                        .GetRequiredService<IServiceScopeFactory>()
                                        .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<T>();

            foreach (var pendingMigration in context.Database.GetPendingMigrations())
            {
                logger.LogInformation($"Migration '{pendingMigration}' is missing, and will be applied.");
            }

            context.Database.Migrate();
        }
    }
}