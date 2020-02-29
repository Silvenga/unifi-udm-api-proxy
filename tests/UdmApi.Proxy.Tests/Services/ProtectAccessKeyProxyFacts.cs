using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using UdmApi.Proxy.Helpers;
using UdmApi.Proxy.Sessions;
using Xunit;

namespace UdmApi.Proxy.Tests.Services
{
    public class ProtectAccessKeyProxyFacts : IClassFixture<UdmApiFactory<Startup>>
    {
        private static readonly Fixture AutoFixture = new Fixture();

        private readonly UdmApiFactory<Startup> _factory;

        public ProtectAccessKeyProxyFacts(UdmApiFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task METHOD_NAME()
        {
            var session = AutoFixture.Create<string>();

            var sessionCache = _factory.SsoSessionCache;
            sessionCache.Add(session);

            var client = _factory.CreateClient();

            // Act
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/auth/access-key")
            {
                Headers =
                {
                    Authorization = AuthenticationHeaderValue.Parse($"Bearer {session}")
                }
            });

            // Assert
            var httpClient = _factory.ProxyHttpClientFactory;
        }
    }

    public class UdmApiFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public IProxyHttpClientFactory ProxyHttpClientFactory { get; } = Substitute.For<IProxyHttpClientFactory>();

        public HttpClient UdmServerHttpClient { get; } = Substitute.ForPartsOf<HttpClient>();

        public ISsoSessionCache SsoSessionCache { get; } = new SsoSessionCache();

        public UdmApiFactory()
        {
            ProxyHttpClientFactory.Create(Arg.Any<bool>()).Returns(UdmServerHttpClient);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(collection =>
            {
                collection.Replace(ServiceDescriptor.Singleton(ProxyHttpClientFactory));
                collection.Replace(ServiceDescriptor.Singleton(SsoSessionCache));
            });
            base.ConfigureWebHost(builder);
        }
    }
}