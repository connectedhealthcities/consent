using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Consent
{
    public class JsonSerializationTests
    {
        private readonly ITestOutputHelper output;
        private TypeRegistry<Evidence, EvidenceAttribute> registry;
        private JsonSerializerSettings serializerSettings;

        /// <inheritdoc />
        public JsonSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
            registry = new EvidenceRegistry();
            serializerSettings = registry.CreateSerializerSettings();
        }

        [Fact]
        public void WhatDoesJsonLookLike()
        {
            
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new ConsentSpecification(),
                    serializerSettings)
            );
        }

        [Fact]
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