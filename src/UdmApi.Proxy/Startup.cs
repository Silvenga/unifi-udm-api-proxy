using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Services;

namespace UdmApi.Proxy
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<AuthenticationProxy>();
            services.AddTransient<ProtectProxy>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.AddServiceProxy<AuthenticationProxy>();
            app.AddServiceProxy<ProtectProxy>();
        }
    }
}