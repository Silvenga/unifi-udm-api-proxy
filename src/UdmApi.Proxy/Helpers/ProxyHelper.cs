using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace UdmApi.Proxy.Helpers
{
    public static class ProxyHelper
    {
        public static void CopyAuthorizationHeaderToCookies(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            if (originalRequest.Headers.TryGetValue("Authorization", out var authorizationHeader)
                && AuthenticationHeaderValue.TryParse(authorizationHeader[0], out var authorizationHeaderValue))
            {
                var token = authorizationHeaderValue.Parameter;
                proxyRequest.Headers.Add("Cookie", $"TOKEN={token}");
            }
        }

        public static void CopyTokenCookieToAuthorizationHeader(HttpResponse response)
        {
            if (response.Headers.TryGetValue("Set-Cookie", out var cookieHeader)
                && CookieHeaderValue.TryParseList(cookieHeader, out var cookies))
            {
                var tokenCookie = cookies.SingleOrDefault(x => x.Name.Equals("TOKEN", StringComparison.OrdinalIgnoreCase));
                var token = tokenCookie?.Value;
                if (token != null)
                {
                    response.Headers.Add("Authorization", $"Bearer {token}");
                }
            }
        }
    }
}