using System;
using CHC.Consent.Api.Client;
using CHC.Consent.DataTool.Features.ImportData;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests
{
    [Collection(WebServerCollection.Name),Trait("Category", "Integration")]
    public class XmlParserTests 
    {
        private WebServerFixture Fixture { get; }
        private ITestOutputHelper Output { get;  }

        public XmlParserTests(ITestOutputHelper output, WebServerFixture fixture)
        {
            Output = output;
            Fixture = fixture;
            Fixture.Output = output;
        }

        [Fact]
        public void CanCreateXmlParserFromMetaData()
        {
            var identityStoreMetadata = Fixture.ApiClient.GetIdentityStoreMetadata();
            var consentStoreMetadata = Fixture.ApiClient.GetConsentStoreMetadata();
            
            Action createParser = () => new XmlParser(
                identityStoreMetadata,
                consentStoreMetadata);
            
            createParser.Should().NotThrow();
        }
    }
}