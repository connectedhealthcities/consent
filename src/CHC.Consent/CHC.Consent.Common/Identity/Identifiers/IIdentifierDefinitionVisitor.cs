using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IIdentifierDefinitionVisitor
    {
        void Visit(DateIdentifierType type);
        void Visit(EnumIdentifierType type);
        void Visit(CompositeIdentifierType type);
        void Visit(IntegerIdentifierType type);
        void Visit(StringIdentifierType type);
        void Visit(IdentifierDefinition type);
    }
}