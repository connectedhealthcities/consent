using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using CHC.Consent.Testing.Utils.Data;
using Microsoft.Rest;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api
{
    [Collection(WebServerCollection.Name)]
    public class IdentityControllerTests
    {
        private readonly WebServerFixture serverFixture;
        private readonly ITestOutputHelper output;
        
        

        /// <inheritdoc />
        public IdentityControllerTests(WebServerFixture serverFixture, ITestOutputHelper output)
        {
            this.serverFixture = serverFixture;
            serverFixture.Output = output;
            this.output = output;
        }
        
        [Fact]
        public async void HandlesPutPerson()
        {
            var client = serverFixture.Client;
            
            var stringContent = new StringContent(
                Data.PersonSpecificationJson,
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                "/identities",
                stringContent);

            if (!Equals(HttpStatusCode.Created, response.StatusCode))
            {
                output.WriteLine(response.AsFormattedString());
            }
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}