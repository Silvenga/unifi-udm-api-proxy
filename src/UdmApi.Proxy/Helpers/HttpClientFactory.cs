using System;
using System.Net.Http;

namespace UdmApi.Proxy.Helpers
{
    public interface IProxyHttpClientFactory
    {
        HttpClient Create(bool disableTlsVerification);
    }

    public class ProxyHttpClientFactory : IProxyHttpClientFactory
    {
        public HttpClient Create(bool disableTlsVerification)
        {
            var handler = new HttpClientHandler();
            if (disableTlsVerification)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            handler.AllowAutoRedirect = false;
            handler.UseCookies = false;

            var httpClient = new HttpClient(handler, true)
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            return httpClient;
        }
    }
}