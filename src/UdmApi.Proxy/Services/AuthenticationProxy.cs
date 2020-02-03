using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UdmApi.Proxy.Helpers;

namespace UdmApi.Proxy.Services
{
    public class AuthenticationProxy : IServiceProxy
    {
        private readonly Uri _udmHost;

        // https://192.168.0.1/proxy/protect/api/bootstrap

        public AuthenticationProxy(IConfiguration configuration)
        {
            _udmHost = configuration.GetValue<Uri>("Udm:Uri");
        }

        public bool DisableTlsVerification() => true;

        public bool Matches(HttpRequest request) => request.Path.StartsWithSegments("/api/auth");

        public void ModifyRequest(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            string rewritePath;
            switch (originalRequest.Path)
            {
                case "/api/auth":
                    rewritePath = "/api/auth/login";
                    break;
                case "/api/auth/access-key":
                    rewritePath = "/proxy/protect/api/bootstrap";
                    break;
                default:
                    rewritePath = originalRequest.Path;
                    break;
            }

            var builder = new UriBuilder(_udmHost)
            {
                Path = rewritePath,
                Query = originalRequest.QueryString.ToString()
            };

            // Gives a 404 when the token cookie is sent for some reason.
            proxyRequest.Headers.Remove("Cookie");
            proxyRequest.Content?.Headers.Remove("Cookie");

            proxyRequest.RequestUri = builder.Uri;

            ProxyHelper.CopyAuthorizationHeaderToCookies(originalRequest, proxyRequest);
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