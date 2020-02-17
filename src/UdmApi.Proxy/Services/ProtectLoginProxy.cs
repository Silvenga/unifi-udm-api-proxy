using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Sessions;

namespace UdmApi.Proxy.Services
{
    public class ProtectLoginProxy : IServiceProxy
    {
        private readonly Uri _udmHost;
        private readonly SsoSessionCache _sessionCache;

        public ProtectLoginProxy(IConfiguration configuration, SsoSessionCache sessionCache)
        {
            _udmHost = configuration.GetValue<Uri>("Udm:Uri");
            _sessionCache = sessionCache;
        }

        public bool DisableTlsVerification() => true;

        public bool Matches(HttpRequest request) => request.Path.Equals("/api/auth") 
                                                    || request.Path.Equals("/api/auth/login");

        public void ModifyRequest(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            var builder = new UriBuilder(_udmHost)
            {
                Path = "/api/auth/login",
                Query = originalRequest.QueryString.ToString()
            };

            // Gives a 404 when the token cookie is sent for some reason.
            proxyRequest.Headers.Remove("Cookie");
            proxyRequest.Content?.Headers.Remove("Cookie");

            proxyRequest.RequestUri = builder.Uri;
        }

        public void ModifyResponseBody(HttpRequest originalRequest, Stream responseBody)
        {
        }

        public void ModifyResponse(HttpRequest originalRequest, HttpResponse response)
        {
            if (response.TryGetSetCookeToken(out var token))
            {
                _sessionCache.Add(token);
                response.Headers.Add("Authorization", token);
            }
        }
    }
}