using System;
using System.Net.Http;
using CHC.Consent.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace CHC.Consent.Tests
{
    public class WebServerFixture : IDisposable
    {
        public TestServer Server { get; }

        public HttpClient Client { get; }

        /// <inheritdoc />
        public WebServerFixture()
        {
            Server = new TestServer(
                new WebHostBuilder().UseStartup<Startup>());
            Client = Server.CreateClient();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            Client?.Dispose();
        }
    }
}