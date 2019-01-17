using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Infrastructure;
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

        /// <inheritdoc />
        public JsonSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
            registry = KnownEvidence.Registry;
            serializerSettings = new JsonSerializerSettings();// registry.CreateSerializerSettings();
        }

        [Fact(Skip = "Work in progress")]
        public void WhatDoesJsonLookLike()
        {
            
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new ConsentSpecification(),
                    serializerSettings)
            );
        }

        [Fact(Skip = "Work in progress")]
        public void CanDeserializeConsentSpecification()
        {
            var roundTripped = JsonConvert.DeserializeObject<ConsentSpecification>(
                JsonConvert.SerializeObject(
                    new ConsentSpecification(),
                    serializerSettings),
                serializerSettings
            );
            
            Assert.NotNull(roundTripped);
            
        }
    }
}