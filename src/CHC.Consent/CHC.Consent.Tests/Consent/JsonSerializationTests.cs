using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Identifiers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Consent
{
    public class JsonSerializationTests
    {
        private readonly ITestOutputHelper output;
        private ConsentIdentifierRegistry registry;
        private JsonSerializerSettings serializerSettings;

        /// <inheritdoc />
        public JsonSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
            registry = new ConsentIdentifierRegistry();
            registry.Add<PregnancyNumberIdentifier>();
            serializerSettings = registry.CreateSerializerSettings();
        }

        [Fact]
        public void WhatDoesJsonLookLike()
        {
            
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new ConsentSpecification
                    {
                        Identifiers = new Identifier[] {new PregnancyNumberIdentifier("testing, testing, 1..2..3")}

                    },
                    serializerSettings)
            );
        }

        [Fact]
        public void CanDeserializeConsentSpecification()
        {
            var roundTripped = JsonConvert.DeserializeObject<ConsentSpecification>(
                JsonConvert.SerializeObject(
                    new ConsentSpecification
                    {
                        Identifiers = new Identifier[] {new PregnancyNumberIdentifier("testing, testing, 1..2..3")}

                    },
                    serializerSettings),
                serializerSettings
            );
            
            Assert.NotNull(roundTripped);
            var identifier = Assert.Single(roundTripped.Identifiers);
            var prenancyNumber = Assert.IsType<PregnancyNumberIdentifier>(identifier);
            Assert.Equal("testing, testing, 1..2..3", prenancyNumber.Value);
        }
    }
}