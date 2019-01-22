using System;
using CHC.Consent.Api.Client;
using CHC.Consent.DataImporter;
using CHC.Consent.DataImporter.Features.ImportData;
using CHC.Consent.Testing.Utils;
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
            var identityStoreMetadata = Fixture.ApiClient.IdentityStoreMetadata();
            var consentStoreMetadata = Fixture.ApiClient.ConsentStoreMetadata();
            
            Action createParser = () => new XmlParser(
                new XunitLogger<XmlParser>(Output, "Parser"),
                identityStoreMetadata,
                consentStoreMetadata);
            
            createParser.Should().NotThrow();
        }
    }
}