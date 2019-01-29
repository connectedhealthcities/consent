using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Testing.Utils;
using CHC.Consent.Tests.Api.Controllers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Integration.Api
{
    public class IdentifierMetaDataControllerTests : WebIntegrationTest
    {
        /// <inheritdoc />
        public IdentifierMetaDataControllerTests(WebServerFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }

        [Fact]
        public void ReturnsCorrectMetaData()
        {
            var identityStoreMetadata = ApiClient.GetIdentityStoreMetadata();

            identityStoreMetadata.Should()
                .Contain(_ => _.SystemName == Identifiers.Definitions.Name.SystemName)
                .Subject.Type.Should().BeEquivalentTo(
                    new CompositeDefinitionType(
                        Identifiers.Definitions.Name.Type.SystemName,
                        new IDefinition[]
                        {
                            new EvidenceDefinition(
                                Identifiers.Definitions.FirstName.SystemName,
                                new StringDefinitionType("string")),
                            new EvidenceDefinition(
                                Identifiers.Definitions.LastName.SystemName,
                                new StringDefinitionType("string")),
                        })
                );
        }
    }
}