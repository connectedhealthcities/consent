using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Tests.Api.Controllers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using EvidenceDefinition = CHC.Consent.Api.Client.Models.EvidenceDefinition;

namespace CHC.Consent.Tests.Integration.Api
{
    public class ConsentMetaDataControllerTests : WebIntegrationTest
    {
        /// <inheritdoc />
        public ConsentMetaDataControllerTests(WebServerFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }

        [Fact]
        public void ReturnsCorrectMetaData()
        {
            var consentStoreMetadata = ApiClient.GetConsentStoreMetadata();

            consentStoreMetadata.Should()
                .Contain(_ => _.SystemName == KnownEvidence.ImportFile.SystemName)
                .Subject.Type.Should().BeEquivalentTo(
                    new CompositeDefinitionType(
                        KnownEvidence.ImportFile.Type.SystemName,
                        new IDefinition[]
                        {
                            new EvidenceDefinition(
                                KnownEvidence.ImportFileParts.BaseUri.SystemName,
                                new StringDefinitionType("string")),
                            new EvidenceDefinition(
                                KnownEvidence.ImportFileParts.LineNumber.SystemName,
                                new IntegerDefinitionType("integer")),
                            new EvidenceDefinition(
                                KnownEvidence.ImportFileParts.LinePosition.SystemName,
                                new IntegerDefinitionType("integer")),
                        })
                );
        }
    }
}