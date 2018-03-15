using System;
using System.Net.Http;
using CHC.Consent.Api;
using CHC.Consent.Testing.Utils;
using CHC.Consent.Tests.Api.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Xunit.Abstractions;

namespace CHC.Consent.Tests
{
    public class WebServerFixture : IDisposable
    {
        private TestOutputLoggerProvider LoggerProvider;
        private XUnitServiceClientTracingInterceptor tracingInterceptor;

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

        public ITestOutputHelper Output
        {
            set
            {
                LoggerProvider.Output = value;
                tracingInterceptor.Output = value;
            }
        }

        public TestServer Server { get; }

        public HttpClient Client { get; }
        
        

        /// <inheritdoc />
        public WebServerFixture()
        {
            LoggerProvider = new TestOutputLoggerProvider();
            Server = new TestServer(
                new WebHostBuilder()
                    .UseStartup<InMemorydatabaseStartup>()
                .ConfigureLogging(_ => _.AddProvider(LoggerProvider))
                );
            Client = Server.CreateClient();
            
            ServiceClientTracing.IsEnabled = true;
            tracingInterceptor = new XUnitServiceClientTracingInterceptor(null);
            ServiceClientTracing.AddTracingInterceptor(tracingInterceptor);
        }

        public class TestOutputLoggerProvider : ILoggerProvider
        {
            public ITestOutputHelper Output { get; set; }

            public TestOutputLoggerProvider()
            {
                
            }

            /// <inheritdoc />
            public void Dispose()
            {
                
            }

            /// <inheritdoc />
            public ILogger CreateLogger(string categoryName)
            {
                return Output == null ? (ILogger) NullLogger.Instance : new XunitLogger(Output, categoryName);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            Client?.Dispose();
        }
    }
}