using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace LobbyPlatformApiGatewayTests.Fixtures
{
    [FixtureLifeCycle(LifeCycle.SingleInstance)]
    [TestFixture]
    internal class ReverseProxyServer : WebApplicationFactory<Program>
    {
        public HttpClient Client { get; }

        public ReverseProxyServer()
        {
            Client = CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
            });

        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder
                //.UseEnvironment("production")
                .ConfigureTestServices(services =>
                {
                
                });
        }

        [OneTimeSetUp] 
        public void OneTimeSetUp() 
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}
