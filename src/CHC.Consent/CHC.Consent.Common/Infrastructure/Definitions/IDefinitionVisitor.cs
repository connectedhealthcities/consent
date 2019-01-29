using CHC.Consent.Common.Infrastructure.Definitions.Types;

namespace CHC.Consent.Common.Infrastructure.Definitions
{
    public interface IDefinitionVisitor
    {
        void Visit(IDefinition definition, DateDefinitionType type);
        void Visit(IDefinition definition, EnumDefinitionType type);
        void Visit(IDefinition definition, CompositeDefinitionType type);
        void Visit(IDefinition definition, IntegerDefinitionType type);
        void Visit(IDefinition definition, StringDefinitionType type);
    }
}