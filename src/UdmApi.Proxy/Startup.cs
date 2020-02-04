using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Services;
using UdmApi.Proxy.Sessions;

namespace UdmApi.Proxy
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ProtectLoginProxy>();
            services.AddTransient<ProtectAccessKeyProxy>();
            services.AddTransient<ProtectProxy>();

            services.AddSingleton<SsoSessionCache>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.AddServiceProxy<ProtectLoginProxy>();
            app.AddServiceProxy<ProtectAccessKeyProxy>();
            app.AddServiceProxy<ProtectProxy>();

            app.UseMiddleware<FallbackMiddleware>();
        }
    }
}