using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UdmApi.Proxy.Helpers;

namespace UdmApi.Proxy.Services
{
    public class ProtectProxy : IServiceProxy
    {
        private readonly Uri _udmHost;

        // https://192.168.0.1/proxy/protect/api/bootstrap

        public ProtectProxy(IConfiguration configuration)
        {
            _udmHost = configuration.GetValue<Uri>("Udm:Uri");
        }

        public bool DisableTlsVerification() => true;

        public bool Matches(HttpRequest request) => request.Path.StartsWithSegments("/api")
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

            if (originalRequest.Headers.TryGetValue("Authorization", out var authorizationHeader)
                && AuthenticationHeaderValue.TryParse(authorizationHeader[0], out var authorizationHeaderValue))
            {
                var token = authorizationHeaderValue.Parameter;
                proxyRequest.Headers.Add("Cookie", $"TOKEN={token}");
            }
        }

        public void ModifyResponseBody(HttpRequest originalRequest, Stream responseBody)
        {
        }

        public void ModifyResponse(HttpRequest originalRequest, HttpResponse response)
        {
            ProxyHelper.CopyTokenCookieToAuthorizationHeader(response);
        }
    }
}