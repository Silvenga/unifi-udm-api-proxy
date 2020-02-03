using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using UdmApi.Proxy.Services;

namespace UdmApi.Proxy
{
    public class ProxyMiddleware<T> where T : IServiceProxy
    {
        // https://github.com/andychiare/netcore2-reverse-proxy/blob/master/ReverseProxyApplication/ReverseProxyMiddleware.cs

        private readonly HttpClient _httpClient;
        private readonly RequestDelegate _nextMiddleware;
        private readonly IServiceProxy _serviceProxy;
        private readonly ILogger<ProxyMiddleware<T>> _logger;

        public ProxyMiddleware(RequestDelegate nextMiddleware, T serviceProxy, ILogger<ProxyMiddleware<T>> logger)
        {
            _nextMiddleware = nextMiddleware;
            _serviceProxy = serviceProxy;
            _logger = logger;

            var handler = new HttpClientHandler();
            if (serviceProxy.DisableTlsVerification())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            handler.AllowAutoRedirect = false;
            handler.UseCookies = false;

            _httpClient = new HttpClient(handler, true)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task Invoke(HttpContext context)
        {
            if (_serviceProxy.Matches(context.Request))
            {
                _logger.LogInformation($"{context.Request.Path}: Handling request using '{typeof(T)}'.");

                // Build Request
                var proxyRequest = CreateProxyRequest(context);
                _serviceProxy.ModifyRequest(context.Request, proxyRequest);
                //proxyRequest.Headers.Host = proxyRequest.RequestUri.Host;

                _logger.LogInformation($"{context.Request.Path}: Proxing request to '{proxyRequest.RequestUri}'.");

                // Send Request
                using var responseMessage = await _httpClient.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

                // Build our response.
                context.Response.StatusCode = (int) responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(responseMessage, context);

                _serviceProxy.ModifyResponse(context.Request, context.Response);

                var contentStream = await responseMessage.Content.ReadAsStreamAsync();
                _serviceProxy.ModifyResponseBody(context.Request, contentStream);
                await contentStream.CopyToAsync(context.Response.Body);

                _logger.LogInformation($"{context.Request.Path}: Proxied with the result of '{context.Response.StatusCode}'.");

                return;
            }

            await _nextMiddleware(context);
        }

        private static HttpRequestMessage CreateProxyRequest(HttpContext context)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            var targetUri = new Uri(context.Request.GetEncodedUrl());

            requestMessage.RequestUri = targetUri;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private static void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;

                foreach (var (key, value) in context.Request.Headers)
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation(key, value.ToArray());
                }
            }
            else
            {
                foreach (var (key, value) in context.Request.Headers)
                {
                    requestMessage.Headers.TryAddWithoutValidation(key, value.ToArray());
                }
            }
        }

        private static void CopyFromTargetResponseHeaders(HttpResponseMessage responseMessage, HttpContext context)
        {
            foreach (var (key, value) in responseMessage.Headers)
            {
                context.Response.Headers[key] = value.ToArray();
            }

            foreach (var (key, value) in responseMessage.Content.Headers)
            {
                context.Response.Headers[key] = value.ToArray();
            }

            context.Response.Headers.Remove("transfer-encoding");
        }

        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }
    }
}