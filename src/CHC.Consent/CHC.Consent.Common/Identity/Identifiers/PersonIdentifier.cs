namespace CHC.Consent.Common.Identity.Identifiers
{
    public class PersonIdentifier : IPersonIdentifier
    {
        public IdentifierValue Value { get; }
        public IdentifierDefinition Definition { get; }

        public PersonIdentifier(IdentifierValue value, IdentifierDefinition definition)
        {
            Value = value;
            Definition = definition;
        }
    }
}