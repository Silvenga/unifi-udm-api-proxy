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
        public static bool TryGetAuthorizationHeader(this HttpRequest request, out string token)
        {
            token = default;

            if (request.Headers.TryGetValue("Authorization", out var authorizationHeader)
                && AuthenticationHeaderValue.TryParse(authorizationHeader[0], out var authorizationHeaderValue))
            {
                token = authorizationHeaderValue.Parameter;
            }

            return token != default;
        }

        public static bool TryGetAccessKeyQueryString(this HttpRequest request, out string token)
        {
            token = default;

            if (request.Query.TryGetValue("accessKey", out var stringValues))
            {
                token = stringValues.FirstOrDefault();
            }

            return token != default;
        }

        public static void CopyAuthorizationHeaderToCookies(HttpRequest originalRequest, HttpRequestMessage proxyRequest)
        {
            if (originalRequest.TryGetAuthorizationHeader(out var token))
            {
                proxyRequest.Headers.Add("Cookie", $"TOKEN={token}");
            }
        }

        public static bool TryGetSetCookeToken(this HttpResponse response, out string token)
        {
            if (response.Headers.TryGetValue("Set-Cookie", out var cookieHeader)
                && CookieHeaderValue.TryParseList(cookieHeader, out var cookies))
            {
                var tokenCookie = cookies.SingleOrDefault(x => x.Name.Equals("TOKEN", StringComparison.OrdinalIgnoreCase));
                token = tokenCookie?.Value.ToString();
            }
            else
            {
                token = default;
            }

            return token != default;
        }

        public static void CopyTokenCookieToAuthorizationHeader(HttpResponse response)
        {
            if (response.TryGetSetCookeToken(out var token))
            {
                response.Headers.Add("Authorization", token);
            }
        }
    }
}