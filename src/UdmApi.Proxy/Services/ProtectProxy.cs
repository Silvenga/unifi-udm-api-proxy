using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Sessions;

namespace UdmApi.Proxy.Services
{
    public class ProtectProxy : IServiceProxy
    {
        private readonly Uri _udmHost;
        private readonly ISsoSessionCache _sessionCache;

        public ProtectProxy(IConfiguration configuration, ISsoSessionCache sessionCache)
        {
            _udmHost = configuration.GetValue<Uri>("Udm:Uri");
            _sessionCache = sessionCache;
        }

        public bool DisableTlsVerification() => true;

        public bool Matches(HttpRequest request) => request.TryGetAuthorizationHeader(out var currentToken) // Only handled active sessions that we know about.
                                                    && _sessionCache.TryGet(currentToken, out _)
                                                    && request.Path.StartsWithSegments("/api")
                                                    && !request.Path.StartsWithSegments("/api/auth");

        public void ModifyRequest(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            originalRequest.Path.StartsWithSegments("/api", out var remaining);
            var builder = new UriBuilder(_udmHost)
            {
                Path = "/proxy/protect/api" + remaining,
                Query = originalRequest.QueryString.ToString()
            };

            // When cookies are sent, the server timeouts.
            proxyRequest.Headers.Remove("Cookie");
            proxyRequest.Content?.Headers.Remove("Cookie");

            proxyRequest.RequestUri = builder.Uri;

            if (originalRequest.TryGetAuthorizationHeader(out var token)
                && _sessionCache.TryGet(token, out var currentToken))
            {
                proxyRequest.Headers.Add("Cookie", $"TOKEN={currentToken}");
            }
        }

        public void ModifyResponseBody(HttpRequest originalRequest, HttpResponse contextResponse, Stream responseBody)
        {
        }

        public void ModifyResponse(HttpRequest originalRequest, HttpResponse response)
        {
            if (originalRequest.TryGetAuthorizationHeader(out var token)
                && response.TryGetSetCookeToken(out var currentToken))
            {
                _sessionCache.Update(token, currentToken);
            }
        }
    }
}