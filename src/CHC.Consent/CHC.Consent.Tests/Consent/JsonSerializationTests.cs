using CHC.Consent.Api;
using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Testing.Utils;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;


namespace CHC.Consent.Tests.Consent
{
    public class JsonSerializationTests
    {
        private readonly ITestOutputHelper output;
        private EvidenceDefinitionRegistry registry;
        private JsonSerializerSettings serializerSettings;
        private readonly ConsentSpecification consentSpecification;

        /// <inheritdoc />
        public JsonSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
            registry = KnownEvidence.Registry;
            serializerSettings = ConfigureJsonOptions.ConfigureSerializer(
                new JsonSerializerSettings(),
                new XunitLogger<IIdentifierValueDtoJsonConverter>(
                    output,
                    "test"));
            consentSpecification = new ConsentSpecification
            {
                Evidence = 
                    new[] { 
                        Evidences.ServerMedwayDto(givenBy:"Self", takenBy:"Nurse Brown"),
                        Evidences.ServerImportFileDto("test.xml", line: 15, offset: 12), 
                    }
            };
        }

        [Fact]
        public void WhatDoesJsonLookLike()
        {
            
            output.WriteLine(
                JsonConvert.SerializeObject(
                    consentSpecification,
                    serializerSettings)
            );
        }

        [Fact()]
        public void CanDeserializeConsentSpecification()
        {
            var roundTripped = JsonConvert.DeserializeObject<ConsentSpecification>(
                JsonConvert.SerializeObject(
                    consentSpecification,
                    serializerSettings),
                serializerSettings
            );
            
            Assert.NotNull(roundTripped);
        }
    }
}