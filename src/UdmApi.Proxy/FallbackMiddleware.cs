using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UdmApi.Proxy
{
    public class FallbackMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;
        private readonly ILogger<FallbackMiddleware> _logger;

        public FallbackMiddleware(RequestDelegate nextMiddleware, ILogger<FallbackMiddleware> logger)
        {
            _nextMiddleware = nextMiddleware;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogWarning($"{context.Request.Path}: Failed to handle request, no handlers matched the request.");
            await _nextMiddleware(context);
        }
    }
}