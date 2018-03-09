using System;
using System.Net.Http;
using CHC.Consent.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CHC.Consent.Tests
{
    public class WebServerFixture : IDisposable
    {
        // used as type parameter
        // ReSharper disable once ClassNeverInstantiated.Local
        private class InMemorydatabaseStartup : Startup
        {
            /// <inheritdoc />
            public InMemorydatabaseStartup(IConfiguration configuration) : base(configuration)
            {
            }

            /// <inheritdoc />
            protected override void ConfigureDatabaseOptions(IServiceProvider provider, DbContextOptionsBuilder options)
            {
                options.UseInMemoryDatabase("CHC-Testing");
            }
        }
        
        public TestServer Server { get; }

        public HttpClient Client { get; }

        /// <inheritdoc />
        public WebServerFixture()
        {
            Server = new TestServer(
                new WebHostBuilder().UseStartup<InMemorydatabaseStartup>());
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