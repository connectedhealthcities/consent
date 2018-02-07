namespace CHC.Consent.Common.Identity
{
    public abstract class IdentifierValueType
    {
        public abstract Identifier Parse(string value, IdentifierType identifierType);
    }
}