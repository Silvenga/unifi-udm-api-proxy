using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace UdmApi.Proxy.Services
{
    public interface IServiceProxy
    {
        bool Matches(HttpRequest request);

        void ModifyRequest(HttpRequest originalRequest, HttpRequestMessage proxyRequest);

        void ModifyResponseBody(HttpRequest originalRequest, Stream responseBody);

        void ModifyResponse(HttpRequest originalRequest, HttpResponse response);

        bool DisableTlsVerification();
    }
}