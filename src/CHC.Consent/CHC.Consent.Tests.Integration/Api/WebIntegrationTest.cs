using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Controllers
{
    [Collection(WebServerCollection.Name), Trait("Category", "Integration")]
    public class WebIntegrationTest
    {
        /// <inheritdoc />
        public WebIntegrationTest(WebServerFixture fixture, ITestOutputHelper output)
        {
            Output = output;
            fixture.Output = output;
            Server = fixture.Server;
            ApiClient = fixture.ApiClient;
        }

        protected ITestOutputHelper Output { get; }
        
        protected TestServer Server { get; }
        protected Consent.Api.Client.Api ApiClient { get; }
    }
}