using Microsoft.AspNetCore.Builder;
using UdmApi.Proxy.Services;

namespace UdmApi.Proxy.Helpers
{
    public static class MiddlewareExtensions
    {
        public static void AddServiceProxy<T>(this IApplicationBuilder app) where T : IServiceProxy
        {
            app.UseMiddleware<ProxyMiddleware<T>>();
        }
    }
}