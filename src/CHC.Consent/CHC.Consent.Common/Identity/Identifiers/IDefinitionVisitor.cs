namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IDefinitionVisitor
    {
        void Visit(IDefinition definition, DateIdentifierType type);
        void Visit(IDefinition definition, EnumIdentifierType type);
        void Visit(IDefinition definition, CompositeIdentifierType type);
        void Visit(IDefinition definition, IntegerIdentifierType type);
        void Visit(IDefinition definition, StringIdentifierType type);
    }
}