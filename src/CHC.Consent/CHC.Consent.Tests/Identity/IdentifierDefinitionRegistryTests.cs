using System;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using FakeItEasy;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class IdentifierDefinitionRegistryTests
    {
        private readonly IdentifierDefinitionRegistry registry = new IdentifierDefinitionRegistry();

        [Fact]
        public void WhenRegisteringAnIdentifier_ItIsRegisteredByKebabCase()
        {
            var definition = new IdentifierDefinition("Test Me", new StringIdentifierType());
            
            registry.Add(definition);
            
            Assert.Equal(definition, registry["test-me"]);
        }
    }
}