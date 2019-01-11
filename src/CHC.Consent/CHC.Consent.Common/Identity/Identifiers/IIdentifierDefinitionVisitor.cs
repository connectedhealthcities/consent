using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IIdentifierDefinitionVisitor
    {
        void Visit(IdentifierDefinition definition, DateIdentifierType type);
        void Visit(IdentifierDefinition definition, EnumIdentifierType type);
        void Visit(IdentifierDefinition definition, CompositeIdentifierType type);
        void Visit(IdentifierDefinition definition, IntegerIdentifierType type);
        void Visit(IdentifierDefinition definition, StringIdentifierType type);
    }
}