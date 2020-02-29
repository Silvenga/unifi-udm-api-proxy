using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Sessions;

namespace UdmApi.Proxy.Services
{
    public class ProtectCameraProxy : IServiceProxy
    {
        private readonly Uri _udmHost;
        private readonly ISsoSessionCache _sessionCache;

        public ProtectCameraProxy(IConfiguration configuration, ISsoSessionCache sessionCache)
        {
            _udmHost = configuration.GetValue<Uri>("Udm:Uri");
            _sessionCache = sessionCache;
        }

        public bool DisableTlsVerification() => true;

        public bool Matches(HttpRequest request) => (request.TryGetAuthorizationHeader(out var token) // Only handled active sessions that we know about.
                                                     || request.TryGetAccessKeyQueryString(out token))
                                                    && _sessionCache.TryGet(token, out _)
                                                    && request.Path.StartsWithSegments("/cameras");

        public void ModifyRequest(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            // https://192.168.0.1/cameras/5e3689d3003aef03870003f3
            // https://192.168.0.1/proxy/protect/api/cameras/5e3689d3003aef03870003f3

            // When cookies are sent, the server timeouts.
            proxyRequest.Headers.Remove("Cookie");
            proxyRequest.Content?.Headers.Remove("Cookie");

            originalRequest.Path.StartsWithSegments("/cameras", out var remaining);
            var builder = new UriBuilder(_udmHost)
            {
                Path = "/proxy/protect/api/cameras" + remaining,
                Query = originalRequest.QueryString.ToString()
            };

            proxyRequest.RequestUri = builder.Uri;

            if ((originalRequest.TryGetAuthorizationHeader(out var token)
                 || originalRequest.TryGetAccessKeyQueryString(out token))
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