using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Common.Identity
{
    public class IdentifierDefinitionRegistryProvider
    {
        private readonly IdentifierDefinitionRegistry identifiers = new IdentifierDefinitionRegistry
        {
            new IdentifierDefinition(name: "NHS Number", type: new StringIdentifierType()),
            new IdentifierDefinition(name: "Sex", type: new EnumIdentifierType("Female", "Male")),
            new IdentifierDefinition(name: "Date Of Birth", type: new DateIdentifierType()),
            new IdentifierDefinition(name: "Bradford Hospital Number", type: new StringIdentifierType()),
            new IdentifierDefinition(
                name: "Address",
                type: new CompositeIdentifierType(
                    new IdentifierDefinition("Line 1", new StringIdentifierType()),
                    new IdentifierDefinition("Line 2", new StringIdentifierType()),
                    new IdentifierDefinition("Line 3", new StringIdentifierType()),
                    new IdentifierDefinition("Line 4", new StringIdentifierType()),
                    new IdentifierDefinition("Line 5", new StringIdentifierType()),
                    new IdentifierDefinition("Postcode", new StringIdentifierType())
                )),
            new IdentifierDefinition(
                name: "Birth Order",
                type: new CompositeIdentifierType(
                    new IdentifierDefinition(name: "Pregnancy Number", type: new IntegerIdentifierType()),
                    new IdentifierDefinition(name: "Birth Order", type: new IntegerIdentifierType())
                )),
            new IdentifierDefinition(
                name: "Name",
                type: new CompositeIdentifierType(
                    new IdentifierDefinition("Given", new StringIdentifierType()),
                    new IdentifierDefinition("Family", new StringIdentifierType())
                ))
        };

        /// <inheritdoc />
        public IdentifierDefinitionRegistryProvider(IdentifierDefinitionRegistry identifiers=null)
        {
            this.identifiers = identifiers ?? this.identifiers;
        }

        public IdentifierDefinitionRegistry GetRegistry() => identifiers;
    }
}