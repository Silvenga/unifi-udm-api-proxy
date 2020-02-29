using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Sessions;

namespace UdmApi.Proxy.Services
{
    public class ProtectAccessKeyProxy : IServiceProxy
    {
        private readonly Uri _udmHost;
        private readonly ISsoSessionCache _ssoSessionCache;

        public ProtectAccessKeyProxy(IConfiguration configuration, ISsoSessionCache ssoSessionCache)
        {
            _udmHost = configuration.GetValue<Uri>("Udm:Uri");
            _ssoSessionCache = ssoSessionCache;
        }

        public bool DisableTlsVerification() => true;

        public bool Matches(HttpRequest request) => request.TryGetAuthorizationHeader(out var currentToken) // Only handled active sessions that we know about.
                                                    && _ssoSessionCache.TryGet(currentToken, out _)
                                                    && request.Path.Equals("/api/auth/access-key");

        public void ModifyRequest(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            // Gives a 404 when the token cookie is sent for some reason.
            proxyRequest.Headers.Remove("Cookie");
            proxyRequest.Content?.Headers.Remove("Cookie");

            var builder = new UriBuilder(_udmHost)
            {
                Path = "/proxy/protect/api/bootstrap",
                Query = originalRequest.QueryString.ToString()
            };

            proxyRequest.RequestUri = builder.Uri;
            proxyRequest.Method = HttpMethod.Get;
            proxyRequest.Content = null;

            if (originalRequest.TryGetAuthorizationHeader(out var token)
                && _ssoSessionCache.TryGet(token, out var currentToken))
            {
                proxyRequest.Headers.Add("Cookie", $"TOKEN={currentToken}");
            }
        }

        public void ModifyResponseBody(HttpRequest originalRequest, HttpResponse contextResponse, Stream responseBody)
        {
            if (contextResponse.StatusCode == 200
                && originalRequest.TryGetAuthorizationHeader(out var token))
            {
                var accessKeyResponse = new
                {
                    accessKey = token
                };
                var json = JsonConvert.SerializeObject(accessKeyResponse);

                responseBody.SetLength(0);

                using (var writer = new StreamWriter(responseBody, leaveOpen: true))
                {
                    writer.Write(json);
                    writer.Flush();
                }

                responseBody.Position = 0;
            }
        }

        public void ModifyResponse(HttpRequest originalRequest, HttpResponse response)
        {
            if (originalRequest.TryGetAuthorizationHeader(out var token)
                && response.TryGetSetCookeToken(out var currentToken))
            {
                _ssoSessionCache.Update(token, currentToken);
            }
        }
    }
}