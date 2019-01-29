using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class IdentifierDefinitionRegistryTests
    {
        private readonly IdentifierDefinitionRegistry registry = new IdentifierDefinitionRegistry();

        [Fact]
        public void WhenRegisteringAnIdentifier_ItIsRegisteredByKebabCase()
        {
            var definition = new IdentifierDefinition("Test Me", new StringDefinitionType());
            
            registry.Add(definition);
            
            Assert.Equal(definition, registry["test-me"]);
        }
    }
}